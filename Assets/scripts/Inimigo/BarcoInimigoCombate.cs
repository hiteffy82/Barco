using UnityEngine;

namespace TrilloBit3sIndieGames
{
    [RequireComponent(typeof(Rigidbody))]
    public class BarcoInimigoCombate : MonoBehaviour
    {
        private enum EstadoIA
        {
            Patrulha,
            Combate,
            BuscarMunicao
        }

        private enum EstadoPatrulha
        {
            Andando,
            Girando,
            Parado
        }

        private EstadoIA estadoAtual;
        private EstadoPatrulha estadoPatrulha;

        //Memoria de detecção
        private float tempoUltimaVisao;
        public float tempoMemoria = 5f;

        private float tempoAcao;

        private float orbitSide = 1f;

        [Header("Canhões")]
        public ShipCannonInimigo cannonDireita;
        public ShipCannonInimigo cannonEsquerda;

        [Header("Alvo")]
        public Transform player;

        [Header("Visão")]
        public float viewDistance = 40f;
        public LayerMask obstacleMask;

        [Header("Combate")]
        public float fireRange = 25f;
        public float fireCooldown = 3f;

        private float lastFireTime;

        [Header("Movimento")]
        public float maxSpeed = 8f;
        public float acceleration = 5f;

        [Header("Direção")]
        public float steerPower = 3f;
        public float steerSmooth = 5f;

        [Header("Rotação Avançada")]
        public float turnMultiplier = 1.5f;     // multiplica força de giro
        public float turnResponsiveness = 2f;   // quão rápido responde ao input
        public float maxTurnTorque = 10f;       // limite de torque
        public float minTurnTorque = 0.5f;      // torque mínimo (evita travar)

        [Header("Munição")]
        public float searchAmmoRadius = 50f;

        // Array para armazenar todas as munições
        public Transform[] allAmmo;

        [Header("Visão Lateral")]
        public float sideViewAngle = 90f; // abertura lateral
        public float sideViewDistance = 35f;

        private Transform ammoTarget;

        private Rigidbody rb;
        private float currentSteer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                    player = p.transform;
            }

            estadoAtual = EstadoIA.Patrulha;
            estadoPatrulha = EstadoPatrulha.Andando;
            orbitSide = Random.value > 0.5f ? 1f : -1f;
            tempoAcao = Random.Range(2f, 5f);

            // PREENCHE ARRAY DE MUNIÇÃO
            GameObject[] ammos = GameObject.FindGameObjectsWithTag("Ammo");
            allAmmo = new Transform[ammos.Length];
            for (int i = 0; i < ammos.Length; i++)
                allAmmo[i] = ammos[i].transform;
        }

        void FixedUpdate()
        {
            if (player == null) return;

            switch (estadoAtual)
            {
                case EstadoIA.Patrulha:
                    AtualizarPatrulha();

                    if (PlayerDetectado())
                        estadoAtual = EstadoIA.Combate;

                    break;

                case EstadoIA.Combate:
                    if (PlayerDetectado())
                    {
                        tempoUltimaVisao = Time.time;
                    }
                    else
                    {
                        // perdeu o player recentemente → ainda tenta achar
                        if (Time.time - tempoUltimaVisao > tempoMemoria)
                        {
                            estadoAtual = EstadoIA.Patrulha;
                            break;
                        }
                    }

                    if (SemMunicao())
                    {
                        estadoAtual = EstadoIA.BuscarMunicao;
                        break;
                    }

                        if (cannonDireita != null) cannonDireita.Mirar(player);
                        if (cannonEsquerda != null) cannonEsquerda.Mirar(player);

                    SeguirPlayer();
                    TentarDisparar();
                    break;

                case EstadoIA.BuscarMunicao:
                    if (!SemMunicao())
                    {
                        estadoAtual = EstadoIA.Combate;
                        break;
                    }

                    ProcurarMunicao();
                    break;
            }

            // impede movimento para trás
            Vector3 localVel = transform.InverseTransformDirection(rb.velocity);

            if (localVel.z < -1f) // permite pequenas correções
            {
                localVel.z = -1f;
                rb.velocity = transform.TransformDirection(localVel);
            }

            Vector3 angVel = rb.angularVelocity;
            angVel.y *= 0.98f; // menos damping → permite girar melhor
            rb.angularVelocity = angVel;
        }

        void ProcurarMunicao()
        {
            // Verifica qual canhão precisa mais
            ShipCannonInimigo alvoCanhao = null;

            if (cannonDireita != null && cannonEsquerda != null)
                alvoCanhao = cannonDireita.currentAmmo < cannonEsquerda.currentAmmo ? cannonDireita : cannonEsquerda;
            else if (cannonDireita != null) alvoCanhao = cannonDireita;
            else if (cannonEsquerda != null) alvoCanhao = cannonEsquerda;

            if (alvoCanhao == null) return;

            // Encontra munição mais próxima do canhão alvo
            if (ammoTarget == null)
            {
                float minDist = Mathf.Infinity;
                Transform melhor = null;

                foreach (Transform ammo in allAmmo)
                {
                    if (ammo == null || !ammo.gameObject.activeInHierarchy) continue;

                    float dist = Vector3.Distance(alvoCanhao.transform.position, ammo.position);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        melhor = ammo;
                    }
                }

                ammoTarget = melhor;
                if (ammoTarget == null) return;
            }

            // Movimento DIRETO PARA A MUNIÇÃO
            Vector3 dir = (ammoTarget.position - transform.position);
            dir.y = 0;

            float distance = dir.magnitude;

            if (distance < 2f)
            {
                ColetarMunicao(ammoTarget);
                ammoTarget = null;
                return;
            }

            dir.Normalize();

            // ROTACIONA SUAVEMENTE PARA O ALVO
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * steerSmooth));

            // MOVE PARA FRENTE SEM DESALINHAR
            Vector3 desiredVelocity = transform.forward * maxSpeed;
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);
        }

        void ColetarMunicao(Transform ammo)
        {
            if (ammo == null) return;

            int quantidade = 5; // quanto cada pickup dá

            if (cannonDireita != null)
                cannonDireita.AddAmmo(quantidade);

            if (cannonEsquerda != null)
                cannonEsquerda.AddAmmo(quantidade);

            // desativa ou destrói a munição do mapa
            ammo.gameObject.SetActive(false);
        }

        void AtualizarPatrulha()
        {
            tempoAcao -= Time.fixedDeltaTime;

            if (tempoAcao <= 0)
            {
                estadoPatrulha = (EstadoPatrulha)Random.Range(0, 3);
                tempoAcao = Random.Range(2f, 5f);
            }

            switch (estadoPatrulha)
            {
                case EstadoPatrulha.Andando:
                    AndarReto();
                    break;

                case EstadoPatrulha.Girando:
                    GirarLeve();
                    AndarDevagar();
                    break;

                case EstadoPatrulha.Parado:
                    Frear();
                    break;
            }
        }

        void AndarDevagar()
        {
            Vector3 desiredVelocity = transform.forward * maxSpeed * 0.3f;
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);
        }

        void GirarLeve()
        {
            float torque = Mathf.Sin(Time.time) * steerPower * 0.5f;
            rb.AddTorque(Vector3.up * torque, ForceMode.Force);
        }

        void Frear()
        {
            Vector3 vel = rb.velocity;
            vel *= 0.98f;
            rb.velocity = vel;
        }

        Transform EncontrarMunicaoMaisProxima()
        {
            GameObject[] ammos = GameObject.FindGameObjectsWithTag("Ammo");

            float minDist = Mathf.Infinity;
            Transform melhor = null;

            foreach (GameObject ammo in ammos)
            {
                if (!ammo.activeInHierarchy) continue;

                Collider c = ammo.GetComponent<Collider>();
                if (c == null || !c.enabled) continue;

                float dist = Vector3.Distance(transform.position, ammo.transform.position);

                if (dist < minDist && dist <= searchAmmoRadius)
                {
                    minDist = dist;
                    melhor = ammo.transform;
                }
            }

            return melhor;
        }

        // DETECÇÃO (SEM FRENTE, SÓ PROXIMIDADE)
        bool PlayerDetectado()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > viewDistance)
                return false;

            // remove ângulo (detecção por proximidade)
            Vector3 dir = (player.position - transform.position).normalized;

            Vector3 origin = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(origin, dir, distance, obstacleMask))
                return false;

            return true;
        }

        void AndarReto()
        {
            Vector3 desiredVelocity = transform.forward * maxSpeed * 0.6f;

            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);

            // leve estabilidade (evita giro aleatório)
            Vector3 angVel = rb.angularVelocity;
            angVel.y *= 0.95f;
            rb.angularVelocity = angVel;
        }

        // MOVIMENTO (posiciona lateralmente)
        void SeguirPlayer()
        {
            Vector3 dirToPlayer = player.position - transform.position;
            dirToPlayer.y = 0;

            float distance = dirToPlayer.magnitude;

            Vector3 dir = dirToPlayer.normalized;

            // Decide lado (mantém consistência)
            float side = Vector3.Dot(transform.right, dir);

            Vector3 orbitDir;

            if (side > 0)
                orbitDir = Vector3.Cross(Vector3.up, dir);   // orbita direita
            else
                orbitDir = Vector3.Cross(dir, Vector3.up);   // orbita esquerda

            // Ajuste de distância (mantém faixa ideal)
            float desiredDistance = 12f;
            float distanceError = distance - desiredDistance;

            Vector3 correction = dir * distanceError * 0.5f;

            // Movimento (continua igual)
            //  DEFINE DIREÇÃO ALVO (orbita + correção)
            Vector3 targetDir = (orbitDir + correction).normalized;

            //  ROTAÇÃO (PRIORIDADE)
            float angle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);

            // quanto mais desalinhado, mais gira
            float steerInput = Mathf.Clamp(angle / 45f, -1f, 1f);

            // força base de giro
            float baseTorque = steerInput * steerPower * turnMultiplier;

            // evita giro fraco demais
            if (Mathf.Abs(baseTorque) < minTurnTorque)
            {
                baseTorque = Mathf.Sign(baseTorque) * minTurnTorque;
            }

            // limita torque máximo
            baseTorque = Mathf.Clamp(baseTorque, -maxTurnTorque, maxTurnTorque);

            //  MOVIMENTO (SÓ FRENTE)
            // reduz velocidade se estiver muito desalinhado
            float alignment = Mathf.Clamp01(1f - Mathf.Abs(angle) / 90f);

            // velocidade final depende do alinhamento
            float speedFactor = Mathf.Lerp(0.2f, 1f, alignment);

            Vector3 desiredVelocity = transform.forward * maxSpeed * speedFactor;
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);
            currentSteer = Mathf.Lerp(currentSteer, baseTorque, Time.fixedDeltaTime * steerSmooth * turnResponsiveness);
            rb.AddTorque(Vector3.up * currentSteer, ForceMode.Force);
        }

        // DISPARO LATERAL (ESSENCIAL)
        void TentarDisparar()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > fireRange)
                return;

            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // ângulo lateral (90° esquerda/direita)
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            // queremos algo próximo de 90 graus
            bool estaNaLateral = angle > 60f && angle < 120f;

            if (!estaNaLateral)
                return;

            // checar lado (esquerda ou direita)
            float side = Vector3.Dot(transform.right, dirToPlayer);

            if (Time.time - lastFireTime > fireCooldown)
            {
                if (side > 0)
                {
                    DispararDireita();
                }
                else
                {
                    DispararEsquerda();
                }

                lastFireTime = Time.time;
            }
        }

        void DispararDireita()
        {
            if (cannonDireita != null && cannonDireita.currentAmmo > 0)
            {
                cannonDireita.Shoot();
                cannonDireita.currentAmmo--;
            }
        }

        void DispararEsquerda()
        {
            if (cannonEsquerda != null && cannonEsquerda.currentAmmo > 0)
            {
                cannonEsquerda.Shoot();
                cannonEsquerda.currentAmmo--;
            }
        }

        bool SemMunicao()
        {
            bool direita = cannonDireita == null || cannonDireita.currentAmmo <= 0;
            bool esquerda = cannonEsquerda == null || cannonEsquerda.currentAmmo <= 0;

            return direita && esquerda; // só busca quando acaba munição
        }

        // busca munição quando esta a baixo
        // bool SemMunicao()
        // {
        //     int total = 0;

        //     if (cannonDireita != null) total += cannonDireita.currentAmmo;
        //     if (cannonEsquerda != null) total += cannonEsquerda.currentAmmo;

        //     return total <= 2; // começa a buscar antes de zerar
        // }

        // DEBUG VISUAL
        void OnDrawGizmosSelected()
        {
            if (player == null) return;

            Vector3 origin = transform.position + Vector3.up * 1.5f;

            float minAngle = 90f - (sideViewAngle * 0.5f);
            float maxAngle = 90f + (sideViewAngle * 0.5f);

            // DESENHAR CONE LATERAL DIREITO
            Gizmos.color = Color.cyan;

            for (float a = minAngle; a <= maxAngle; a += 5f)
            {
                Vector3 dir = Quaternion.Euler(0, a, 0) * transform.forward;
                Gizmos.DrawRay(origin, dir * sideViewDistance);
            }

            // DESENHAR CONE LATERAL ESQUERDO
            for (float a = -maxAngle; a <= -minAngle; a += 5f)
            {
                Vector3 dir = Quaternion.Euler(0, a, 0) * transform.forward;
                Gizmos.DrawRay(origin, dir * sideViewDistance);
            }

            // LINHA ATÉ O PLAYER
            Vector3 dirToPlayer = (player.position - origin).normalized;
            float dist = Vector3.Distance(origin, player.position);

            if (PlayerDetectado())
                Gizmos.color = Color.green; // detectado
            else
                Gizmos.color = Color.red; // fora da visão

            Gizmos.DrawRay(origin, dirToPlayer * dist);

            // busca munição
            if (ammoTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, ammoTarget.position + Vector3.up * 1.5f);
                Gizmos.DrawSphere(ammoTarget.position, 1f);
            }

            // esfera do alcance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sideViewDistance);
        }
    }
} 


/*
using UnityEngine;

namespace TrilloBit3sIndieGames
{
    [RequireComponent(typeof(Rigidbody))]
    public class BarcoInimigoCombate : MonoBehaviour
    {
        private enum EstadoIA
        {
            Patrulha,
            Combate,
            BuscarMunicao
        }

        private enum EstadoPatrulha
        {
            Andando,
            Girando,
            Parado
        }

        private EstadoIA estadoAtual;
        private EstadoPatrulha estadoPatrulha;

        //Memoria de detecção
        private float tempoUltimaVisao;
        public float tempoMemoria = 5f;

        private float tempoAcao;

        private float orbitSide = 1f;

        [Header("Canhões")]
        public ShipCannonInimigo cannonDireita;
        public ShipCannonInimigo cannonEsquerda;

        [Header("Alvo")]
        public Transform player;

        [Header("Visão")]
        public float viewDistance = 40f;
        public LayerMask obstacleMask;

        [Header("Combate")]
        public float fireRange = 25f;
        public float fireCooldown = 3f;

        private float lastFireTime;

        [Header("Movimento")]
        public float maxSpeed = 8f;
        public float acceleration = 5f;

        [Header("Direção")]
        public float steerPower = 3f;
        public float steerSmooth = 5f;

        [Header("Rotação Avançada")]
        public float turnMultiplier = 1.5f;     // multiplica força de giro
        public float turnResponsiveness = 2f;   // quão rápido responde ao input
        public float maxTurnTorque = 10f;       // limite de torque
        public float minTurnTorque = 0.5f;      // torque mínimo (evita travar)

        [Header("Munição")]
        public float searchAmmoRadius = 50f;

        // Array para armazenar todas as munições
        public Transform[] allAmmo;

        [Header("Visão Lateral")]
        public float sideViewAngle = 90f; // abertura lateral
        public float sideViewDistance = 35f;

        private Transform ammoTarget;

        private Rigidbody rb;
        private float currentSteer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                    player = p.transform;
            }

            estadoAtual = EstadoIA.Patrulha;
            estadoPatrulha = EstadoPatrulha.Andando;
            orbitSide = Random.value > 0.5f ? 1f : -1f;
            tempoAcao = Random.Range(2f, 5f);

            // PREENCHE ARRAY DE MUNIÇÃO
            GameObject[] ammos = GameObject.FindGameObjectsWithTag("Ammo");
            allAmmo = new Transform[ammos.Length];
            for (int i = 0; i < ammos.Length; i++)
                allAmmo[i] = ammos[i].transform;
        }

        void FixedUpdate()
        {
            if (player == null) return;

            switch (estadoAtual)
            {
                case EstadoIA.Patrulha:
                    AtualizarPatrulha();

                    if (PlayerDetectado())
                        estadoAtual = EstadoIA.Combate;

                    break;

                case EstadoIA.Combate:
                    if (PlayerDetectado())
                    {
                        tempoUltimaVisao = Time.time;
                    }
                    else
                    {
                        // perdeu o player recentemente → ainda tenta achar
                        if (Time.time - tempoUltimaVisao > tempoMemoria)
                        {
                            estadoAtual = EstadoIA.Patrulha;
                            break;
                        }
                    }

                    if (SemMunicao())
                    {
                        estadoAtual = EstadoIA.BuscarMunicao;
                        break;
                    }

                        if (cannonDireita != null) cannonDireita.Mirar(player);
                        if (cannonEsquerda != null) cannonEsquerda.Mirar(player);

                    SeguirPlayer();
                    TentarDisparar();
                    break;

                case EstadoIA.BuscarMunicao:
                    if (!SemMunicao())
                    {
                        estadoAtual = EstadoIA.Combate;
                        break;
                    }

                    ProcurarMunicao();
                    break;
            }

            // impede movimento para trás
            Vector3 localVel = transform.InverseTransformDirection(rb.velocity);

            if (localVel.z < -1f) // permite pequenas correções
            {
                localVel.z = -1f;
                rb.velocity = transform.TransformDirection(localVel);
            }

            Vector3 angVel = rb.angularVelocity;
            angVel.y *= 0.98f; // menos damping → permite girar melhor
            rb.angularVelocity = angVel;
        }

        void ProcurarMunicao()
        {
            // Verifica qual canhão precisa mais
            ShipCannonInimigo alvoCanhao = null;

            if (cannonDireita != null && cannonEsquerda != null)
                alvoCanhao = cannonDireita.currentAmmo < cannonEsquerda.currentAmmo ? cannonDireita : cannonEsquerda;
            else if (cannonDireita != null) alvoCanhao = cannonDireita;
            else if (cannonEsquerda != null) alvoCanhao = cannonEsquerda;

            if (alvoCanhao == null) return;

            // Encontra munição mais próxima do canhão alvo
            if (ammoTarget == null)
            {
                float minDist = Mathf.Infinity;
                Transform melhor = null;

                foreach (Transform ammo in allAmmo)
                {
                    if (ammo == null || !ammo.gameObject.activeInHierarchy) continue;

                    float dist = Vector3.Distance(alvoCanhao.transform.position, ammo.position);

                    if (dist < minDist)
                    {
                        minDist = dist;
                        melhor = ammo;
                    }
                }

                ammoTarget = melhor;
                if (ammoTarget == null) return;
            }

            // Movimento DIRETO PARA A MUNIÇÃO
            Vector3 dir = (ammoTarget.position - transform.position);
            dir.y = 0;

            float distance = dir.magnitude;

            if (distance < 2f)
            {
                ColetarMunicao(ammoTarget);
                ammoTarget = null;
                return;
            }

            dir.Normalize();

            // ROTACIONA SUAVEMENTE PARA O ALVO
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * steerSmooth));

            // MOVE PARA FRENTE SEM DESALINHAR
            Vector3 desiredVelocity = transform.forward * maxSpeed;
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);
        }

        void ColetarMunicao(Transform ammo)
        {
            if (ammo == null) return;

            int quantidade = 5; // quanto cada pickup dá

            if (cannonDireita != null)
                cannonDireita.AddAmmo(quantidade);

            if (cannonEsquerda != null)
                cannonEsquerda.AddAmmo(quantidade);

            // desativa ou destrói a munição do mapa
            ammo.gameObject.SetActive(false);
        }

        void AtualizarPatrulha()
        {
            tempoAcao -= Time.fixedDeltaTime;

            if (tempoAcao <= 0)
            {
                estadoPatrulha = (EstadoPatrulha)Random.Range(0, 3);
                tempoAcao = Random.Range(2f, 5f);
            }

            switch (estadoPatrulha)
            {
                case EstadoPatrulha.Andando:
                    AndarReto();
                    break;

                case EstadoPatrulha.Girando:
                    GirarLeve();
                    AndarDevagar();
                    break;

                case EstadoPatrulha.Parado:
                    Frear();
                    break;
            }
        }

        void AndarDevagar()
        {
            Vector3 desiredVelocity = transform.forward * maxSpeed * 0.3f;
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);
        }

        void GirarLeve()
        {
            float torque = Mathf.Sin(Time.time) * steerPower * 0.5f;
            rb.AddTorque(Vector3.up * torque, ForceMode.Force);
        }

        void Frear()
        {
            Vector3 vel = rb.velocity;
            vel *= 0.98f;
            rb.velocity = vel;
        }

        Transform EncontrarMunicaoMaisProxima()
        {
            GameObject[] ammos = GameObject.FindGameObjectsWithTag("Ammo");

            float minDist = Mathf.Infinity;
            Transform melhor = null;

            foreach (GameObject ammo in ammos)
            {
                if (!ammo.activeInHierarchy) continue;

                Collider c = ammo.GetComponent<Collider>();
                if (c == null || !c.enabled) continue;

                float dist = Vector3.Distance(transform.position, ammo.transform.position);

                if (dist < minDist && dist <= searchAmmoRadius)
                {
                    minDist = dist;
                    melhor = ammo.transform;
                }
            }

            return melhor;
        }

        // DETECÇÃO (SEM FRENTE, SÓ PROXIMIDADE)
        bool PlayerDetectado()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > viewDistance)
                return false;

            // remove ângulo (detecção por proximidade)
            Vector3 dir = (player.position - transform.position).normalized;

            Vector3 origin = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(origin, dir, distance, obstacleMask))
                return false;

            return true;
        }

        void AndarReto()
        {
            Vector3 desiredVelocity = transform.forward * maxSpeed * 0.6f;

            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);

            // leve estabilidade (evita giro aleatório)
            Vector3 angVel = rb.angularVelocity;
            angVel.y *= 0.95f;
            rb.angularVelocity = angVel;
        }

        // MOVIMENTO (posiciona lateralmente)
        void SeguirPlayer()
        {
            Vector3 dirToPlayer = player.position - transform.position;
            dirToPlayer.y = 0;

            float distance = dirToPlayer.magnitude;

            Vector3 dir = dirToPlayer.normalized;

            // Decide lado (mantém consistência)
            float side = Vector3.Dot(transform.right, dir);

            Vector3 orbitDir;

            if (side > 0)
                orbitDir = Vector3.Cross(Vector3.up, dir);   // orbita direita
            else
                orbitDir = Vector3.Cross(dir, Vector3.up);   // orbita esquerda

            // Ajuste de distância (mantém faixa ideal)
            float desiredDistance = 12f;
            float distanceError = distance - desiredDistance;

            Vector3 correction = dir * distanceError * 0.5f;

            // Movimento (continua igual)
            //  DEFINE DIREÇÃO ALVO (orbita + correção)
            Vector3 targetDir = (orbitDir + correction).normalized;

            //  ROTAÇÃO (PRIORIDADE)
            float angle = Vector3.SignedAngle(transform.forward, targetDir, Vector3.up);

            // quanto mais desalinhado, mais gira
            float steerInput = Mathf.Clamp(angle / 45f, -1f, 1f);

            // força base de giro
            float baseTorque = steerInput * steerPower * turnMultiplier;

            // evita giro fraco demais
            if (Mathf.Abs(baseTorque) < minTurnTorque)
            {
                baseTorque = Mathf.Sign(baseTorque) * minTurnTorque;
            }

            // limita torque máximo
            baseTorque = Mathf.Clamp(baseTorque, -maxTurnTorque, maxTurnTorque);

            //  MOVIMENTO (SÓ FRENTE)
            // reduz velocidade se estiver muito desalinhado
            float alignment = Mathf.Clamp01(1f - Mathf.Abs(angle) / 90f);

            // velocidade final depende do alinhamento
            float speedFactor = Mathf.Lerp(0.2f, 1f, alignment);

            Vector3 desiredVelocity = transform.forward * maxSpeed * speedFactor;
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, acceleration);
            currentSteer = Mathf.Lerp(currentSteer, baseTorque, Time.fixedDeltaTime * steerSmooth * turnResponsiveness);
            rb.AddTorque(Vector3.up * currentSteer, ForceMode.Force);
        }

        // DISPARO LATERAL (ESSENCIAL)
        void TentarDisparar()
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > fireRange)
                return;

            Vector3 dirToPlayer = (player.position - transform.position).normalized;

            // ângulo lateral (90° esquerda/direita)
            float angle = Vector3.Angle(transform.forward, dirToPlayer);

            // queremos algo próximo de 90 graus
            bool estaNaLateral = angle > 60f && angle < 120f;

            if (!estaNaLateral)
                return;

            // checar lado (esquerda ou direita)
            float side = Vector3.Dot(transform.right, dirToPlayer);

            if (Time.time - lastFireTime > fireCooldown)
            {
                if (side > 0)
                {
                    DispararDireita();
                }
                else
                {
                    DispararEsquerda();
                }

                lastFireTime = Time.time;
            }
        }

        void DispararDireita()
        {
            if (cannonDireita != null && cannonDireita.currentAmmo > 0)
            {
                cannonDireita.Shoot();
                cannonDireita.currentAmmo--;
            }
        }

        void DispararEsquerda()
        {
            if (cannonEsquerda != null && cannonEsquerda.currentAmmo > 0)
            {
                cannonEsquerda.Shoot();
                cannonEsquerda.currentAmmo--;
            }
        }

        bool SemMunicao()
        {
            bool direita = cannonDireita == null || cannonDireita.currentAmmo <= 0;
            bool esquerda = cannonEsquerda == null || cannonEsquerda.currentAmmo <= 0;

            return direita && esquerda; // só busca quando acaba munição
        }

        // busca munição quando esta a baixo
        // bool SemMunicao()
        // {
        //     int total = 0;

        //     if (cannonDireita != null) total += cannonDireita.currentAmmo;
        //     if (cannonEsquerda != null) total += cannonEsquerda.currentAmmo;

        //     return total <= 2; // começa a buscar antes de zerar
        // }

        // DEBUG VISUAL
        void OnDrawGizmosSelected()
        {
            if (player == null) return;

            Vector3 origin = transform.position + Vector3.up * 1.5f;

            float minAngle = 90f - (sideViewAngle * 0.5f);
            float maxAngle = 90f + (sideViewAngle * 0.5f);

            // DESENHAR CONE LATERAL DIREITO
            Gizmos.color = Color.cyan;

            for (float a = minAngle; a <= maxAngle; a += 5f)
            {
                Vector3 dir = Quaternion.Euler(0, a, 0) * transform.forward;
                Gizmos.DrawRay(origin, dir * sideViewDistance);
            }

            // DESENHAR CONE LATERAL ESQUERDO
            for (float a = -maxAngle; a <= -minAngle; a += 5f)
            {
                Vector3 dir = Quaternion.Euler(0, a, 0) * transform.forward;
                Gizmos.DrawRay(origin, dir * sideViewDistance);
            }

            // LINHA ATÉ O PLAYER
            Vector3 dirToPlayer = (player.position - origin).normalized;
            float dist = Vector3.Distance(origin, player.position);

            if (PlayerDetectado())
                Gizmos.color = Color.green; // detectado
            else
                Gizmos.color = Color.red; // fora da visão

            Gizmos.DrawRay(origin, dirToPlayer * dist);

            // busca munição
            if (ammoTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, ammoTarget.position + Vector3.up * 1.5f);
                Gizmos.DrawSphere(ammoTarget.position, 1f);
            }

            // esfera do alcance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sideViewDistance);
        }
    }
} 
*/