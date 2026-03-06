// Perfeito 02/12/2025
// preciso ajustar as espumas 
// Buzina, Rotação do Timão, som do motor
// script de barco, já pensado para uma movimentação suave, estável e realista na água, considerando aceleração, giro, empuxo e estabilidade:
//------------------------------------------------------------------------------    
// Seção		  Variável	   		Valor sugerido		Observações
//------------------------------------------------------------------------------
// Referências	 Motor				(Transform do motor)	Usado para animação visual
// Movimentação  MaxSpeed		    8–12				Velocidade máxima para frente. Para barcos pequenos, 8 é realista; para médios, 10–12.
// 				 ReverseSpeed		4–6					Ré geralmente mais lenta.
// 				 ForwardPower		40–60				Força aplicada para alcançar a velocidade; quanto maior, mais responsivo.
// 				 Acceleration		5–10				Quanto maior, mais rápido alcança a velocidade. Evite valores muito altos, deixa brusco.
// 				 Deceleration		2–5					Quanto maior, mais rápido para quando solta o acelerador.
// 				 LateralDrag		0.1–0.3				Reduz deslizamento lateral. Valores maiores → barco “gruda” na direção.
// Direção		 SteerPower			1–3					Torque aplicado para virar. Menor → mais suave; maior → mais responsivo.
// 				 SteerSmooth		0.03–0.08			Quanto maior, mais suave é a rotação. Ajuste fino.
// 				 MinSteerSpeed		0.5–1				Velocidade mínima para começar a virar. Evita giro brusco parado.   
// Estabilidade	 RollStability		2–5					Corrige inclinação lateral. Maior → menos tombamento.
// 				 PitchStability		1–3					Corrige inclinação frontal/traseira. Maior → menos empinar na aceleração.
//
// Dicas de ajuste:
// 1. Se o barco treme → aumente RollStability e PitchStability, e reduza ForwardPower ou Acceleration.
// 2. Se o giro está brusco → aumente SteerSmooth ou reduza SteerPower.
// 3. Se desliza muito lateralmente → aumente LateralDrag.
// 4. Se a aceleração é muito instantânea → reduza Acceleration e aumente ForwardPower para suavizar.

//Sistema com aceleração no RT,R2,LEFT SHIFT
// Perfeito 02/12/2025
// preciso ajustar as espumas 
// Buzina, Rotação do Timão, som do motor
// script de barco, já pensado para uma movimentação suave, estável e realista na água, considerando aceleração, giro, empuxo e estabilidade:
//------------------------------------------------------------------------------    
// Seção		  Variável	   		Valor sugerido		Observações
//------------------------------------------------------------------------------
// Referências	 Motor				(Transform do motor)	Usado para animação visual
// Movimentação  MaxSpeed		    8–12				Velocidade máxima para frente. Para barcos pequenos, 8 é realista; para médios, 10–12.
// 				 ReverseSpeed		4–6					Ré geralmente mais lenta.
// 				 ForwardPower		40–60				Força aplicada para alcançar a velocidade; quanto maior, mais responsivo.
// 				 Acceleration		5–10				Quanto maior, mais rápido alcança a velocidade. Evite valores muito altos, deixa brusco.
// 				 Deceleration		2–5					Quanto maior, mais rápido para quando solta o acelerador.
// 				 LateralDrag		0.1–0.3				Reduz deslizamento lateral. Valores maiores → barco “gruda” na direção.
// Direção		 SteerPower			1–3					Torque aplicado para virar. Menor → mais suave; maior → mais responsivo.
// 				 SteerSmooth		0.03–0.08			Quanto maior, mais suave é a rotação. Ajuste fino.
// 				 MinSteerSpeed		0.5–1				Velocidade mínima para começar a virar. Evita giro brusco parado.   
// Estabilidade	 RollStability		2–5					Corrige inclinação lateral. Maior → menos tombamento.
// 				 PitchStability		1–3					Corrige inclinação frontal/traseira. Maior → menos empinar na aceleração.
//
// Dicas de ajuste:
// 1. Se o barco treme → aumente RollStability e PitchStability, e reduza ForwardPower ou Acceleration.
// 2. Se o giro está brusco → aumente SteerSmooth ou reduza SteerPower.
// 3. Se desliza muito lateralmente → aumente LateralDrag.
// 4. Se a aceleração é muito instantânea → reduza Acceleration e aumente ForwardPower para suavizar.

//Sistema com aceleração no RT,R2,LEFT SHIFT
using UnityEngine;
using UnityEngine.InputSystem;

namespace TrilloBit3sIndieGames
{
    [RequireComponent(typeof(Rigidbody))]
    public class Barquinho : MonoBehaviour
    {
        [Header("Timão")]
        public Transform Timao;
        public float TimaoMaxAngle = 180f;
        public float TimaoLerpSpeed = 5f;
        private Quaternion TimaoRotOriginal;

        [Header("UI")]
        public RectTransform TimaoUI;

        [Header("Referências")]
        public Transform Motor;
        public AudioSource BuzinaAudio;
        public AudioSource MotorAudio;

        [Header("Movimentação")]
        public float MaxSpeed = 100f;
        public float ReverseSpeed = 50f;
        public float Acceleration = 5f;
        private bool turboAtivado = false;
        public float LateralDrag = 0.1f;

        [Header("Direção")]
        public float SteerPower = 1f;
        public float SteerSmooth = 0.01f;
        public float MinSteerSpeed = 1f;

        [Header("Estabilidade")]
        public float RollStability = 3f;

        [Header("Buzina")]
        [Range(0f, 1f)] public float BuzinaVolume = 1f;

        [Header("Motor")]
        [Range(0f, 1f)] public float MotorVolumeMax = 0.5f;
        [Range(0f, 1f)] public float MotorVolumeMin = 0.1f;
        public float VelocidadeParaMaxVolume = 8f;

        [Header("Efeitos")]
        [Header("Espuma Traseira")]
        public ParticleSystem MovimentoParticles;
        public float VelocidadeMinimaParaParticles = 5f;
        public float VelocidadeMaxParaEmission = 10f;
        public float EmissionMaxRate = 50f;

        [Header("Espuma Dianteira Esquerda")]
        public ParticleSystem MovimentoFrontalParticles;
        public float VelocidadeMinimaParaParticlesFrontal = 1f;
        public float EmissionMaxRateFrontal = 40f;

        [Header("Espuma Dianteira Direita")]
        public ParticleSystem MovimentoFrontalParticles2;
        public float VelocidadeMinimaParaParticlesFrontal2 = 1f;
        public float EmissionMaxRateFrontal2 = 40f;

        private Rigidbody rb;
        private float inputForward;
        private float inputSteer;

        [Header("Malha do Oceano")]
        public WaterEffect water;

        void Awake()
        {
            if (Timao != null)
                TimaoRotOriginal = Timao.localRotation;

            rb = GetComponent<Rigidbody>();

            if (BuzinaAudio != null) BuzinaAudio.volume = BuzinaVolume;

            if (MotorAudio != null)
            {
                MotorAudio.loop = true;
                MotorAudio.volume = MotorVolumeMin;
                MotorAudio.Play();
            }
        }

        void Update()
        {
            float kbForward = Input.GetKey(KeyCode.W) ? 1f : 0f;
            float kbReverse = Input.GetKey(KeyCode.S) ? 1f : 0f;
            float kbSteer = Input.GetAxis("Horizontal");

            var gamepad = Gamepad.current;
            float gpForward = 0f;
            float gpReverse = 0f;
            float gpSteer = 0f;

            if (gamepad != null)
            {
                gpForward = gamepad.leftShoulder.isPressed ? 1f : 0f;
                gpReverse = gamepad.leftTrigger.ReadValue();
                gpSteer = gamepad.leftStick.x.ReadValue();
            }

            inputForward = (kbForward - kbReverse) + (gpForward - gpReverse);
            inputSteer = Mathf.Clamp(kbSteer + gpSteer, -1f, 1f);

            bool buzinaAtivada =
                Input.GetKeyDown(KeyCode.B) ||
                (gamepad != null && gamepad.rightShoulder.wasPressedThisFrame);

            if (buzinaAtivada && BuzinaAudio != null && !BuzinaAudio.isPlaying)
            {
                BuzinaAudio.volume = BuzinaVolume;
                BuzinaAudio.Play();
            }

            if (MotorAudio != null)
            {
                float velocidade = rb.velocity.magnitude;
                MotorAudio.volume = Mathf.Lerp(
                    MotorVolumeMin,
                    MotorVolumeMax,
                    Mathf.Clamp01(velocidade / VelocidadeParaMaxVolume)
                );
            }

            bool shiftTurbo = Input.GetKey(KeyCode.LeftShift);
            bool rtTurbo = gamepad != null && gamepad.rightTrigger.ReadValue() > 0.1f;
            turboAtivado = shiftTurbo || rtTurbo;
        }

        void FixedUpdate()
        {
            ApplyBuoyancyStability();
            ApplyMovement();
            ApplySteering();
            ApplyDrag();

            UpdateParticles();
            UpdateFrontParticles();
            UpdateFrontParticles2();
        }

        void ApplyMovement()
        {
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            float targetSpeed = 0f;
            if (inputForward > 0f) targetSpeed = MaxSpeed;
            else if (inputForward < 0f) targetSpeed = -ReverseSpeed;

            if (turboAtivado && inputForward > 0f)
                targetSpeed += 100f;

            Vector3 desiredVelocity = forward * (targetSpeed * Mathf.Abs(inputForward));
            FisicaDoBarquinho.ApplyForceToReachVelocity(rb, desiredVelocity, Acceleration);
        }

        void ApplySteering()
        {
            // Timão SEMPRE anima (mesmo parado)
            AtualizarTimaoVisual();

            // velocidade real na direção do barco
            float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);

            // se não estiver se movendo → não gira o casco
            if (Mathf.Abs(forwardSpeed) < MinSteerSpeed)
            {
                Vector3 angular = rb.angularVelocity;
                angular.y = Mathf.Lerp(angular.y, 0f, SteerSmooth * 3f);
                rb.angularVelocity = angular;
                return;
            }

            float steerFactor = Mathf.Clamp(Mathf.Abs(forwardSpeed) / MaxSpeed, 0.2f, 1f);

            if (!Mathf.Approximately(inputSteer, 0f))
            {
                float analogRotationMultiplier = 0.5f;
                float targetTorque =
                    inputSteer * SteerPower * steerFactor * analogRotationMultiplier;

                //(fisicamente estável):
                rb.AddTorque(Vector3.up * targetTorque, ForceMode.Force);

                //rb.AddTorque(Vector3.up * targetTorque, ForceMode.Acceleration);

                if (Motor != null)
                    Motor.localRotation =
                        Quaternion.Euler(0, inputSteer * 10f * steerFactor, 0) * Motor.localRotation;
            }
        }

        void AtualizarTimaoVisual()
        {
            if (Timao != null)
            {
                float targetZ = inputSteer * -TimaoMaxAngle;
                Quaternion targetRotation = Quaternion.Euler(
                    TimaoRotOriginal.eulerAngles.x,
                    TimaoRotOriginal.eulerAngles.y,
                    targetZ
                );

                Timao.localRotation = Quaternion.Slerp(
                    Timao.localRotation,
                    targetRotation,
                    Time.deltaTime * TimaoLerpSpeed
                );
            }

            if (TimaoUI != null)
            {
                float uiTargetZ = inputSteer * TimaoMaxAngle;
                Quaternion uiTargetRotation = Quaternion.Euler(0, 0, uiTargetZ);

                TimaoUI.localRotation = Quaternion.Slerp(
                    TimaoUI.localRotation,
                    uiTargetRotation,
                    Time.deltaTime * TimaoLerpSpeed
                );
            }
        }

        void ApplyDrag()
        {
            Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
            localVel.x *= (1f - LateralDrag);
            rb.velocity = transform.TransformDirection(localVel);
        }

        void ApplyBuoyancyStability()
        {
            Vector3 predictedUp = Vector3.up;
            Quaternion current = rb.rotation;
            Quaternion target = Quaternion.FromToRotation(transform.up, predictedUp) * current;
            rb.MoveRotation(Quaternion.Slerp(current, target, Time.fixedDeltaTime * RollStability));
        }

        void UpdateParticles()
        {
            if (MovimentoParticles == null) return;

            float velocidade = rb.velocity.magnitude;
            var emission = MovimentoParticles.emission;

            if (velocidade > VelocidadeMinimaParaParticles)
            {
                if (!MovimentoParticles.isPlaying)
                    MovimentoParticles.Play();

                float rate = Mathf.Lerp(0, EmissionMaxRate,
                    Mathf.Clamp01(velocidade / VelocidadeMaxParaEmission));
                emission.rateOverTime = rate;
            }
            else
            {
                if (MovimentoParticles.isPlaying)
                    MovimentoParticles.Stop();
            }
        }

        void UpdateFrontParticles()
        {
            if (MovimentoFrontalParticles == null) return;

            float velocidadeFrente = Mathf.Max(0, Vector3.Dot(rb.velocity, transform.forward));
            var emission = MovimentoFrontalParticles.emission;

            if (velocidadeFrente > VelocidadeMinimaParaParticlesFrontal)
            {
                if (!MovimentoFrontalParticles.isPlaying)
                    MovimentoFrontalParticles.Play();

                float rate = Mathf.Lerp(0, EmissionMaxRateFrontal,
                    Mathf.Clamp01(velocidadeFrente / VelocidadeMaxParaEmission));
                emission.rateOverTime = rate;
            }
            else
            {
                if (MovimentoFrontalParticles.isPlaying)
                    MovimentoFrontalParticles.Stop();
            }
        }

        void UpdateFrontParticles2()
        {
            if (MovimentoFrontalParticles2 == null) return;

            float velocidadeFrente = Mathf.Max(0, Vector3.Dot(rb.velocity, transform.forward));
            var emission = MovimentoFrontalParticles2.emission;

            if (velocidadeFrente > VelocidadeMinimaParaParticlesFrontal2)
            {
                if (!MovimentoFrontalParticles2.isPlaying)
                    MovimentoFrontalParticles2.Play();

                float rate = Mathf.Lerp(0, EmissionMaxRateFrontal2,
                    Mathf.Clamp01(velocidadeFrente / VelocidadeMaxParaEmission));
                emission.rateOverTime = rate;
            }
            else
            {
                if (MovimentoFrontalParticles2.isPlaying)
                    MovimentoFrontalParticles2.Stop();
            }
        }

        void LateUpdate()
        {
            if (water == null) return;

            if (MovimentoFrontalParticles != null)
                AjustarAlturaParticulas(MovimentoFrontalParticles);

            if (MovimentoFrontalParticles2 != null)
                AjustarAlturaParticulas(MovimentoFrontalParticles2);

            if (MovimentoParticles != null)
                AjustarAlturaParticulas(MovimentoParticles);
        }

        void AjustarAlturaParticulas(ParticleSystem ps)
        {
            ParticleSystem.Particle[] particles =
                new ParticleSystem.Particle[ps.main.maxParticles];

            int count = ps.GetParticles(particles);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = particles[i].position;
                pos.y = water.GetHeight(pos);
                particles[i].position = pos;
            }

            ps.SetParticles(particles, count);
        }
    }
}