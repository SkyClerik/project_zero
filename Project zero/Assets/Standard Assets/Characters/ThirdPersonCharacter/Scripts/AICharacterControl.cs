using Fungus;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }
        public ThirdPersonCharacter character { get; private set; }
        public Transform target;

        [SerializeField] private Camera mainCam;

        [Header("Settings")]
        [SerializeField] private float walkSpeed = 0.65f;
        [SerializeField] private float runSpeed = 1.0f;
        [SerializeField] private float doubleClickThreshold = 0.3f;
        [SerializeField] private float holdThreshold = 0.2f;

        private float lastClickTime;
        private float mouseDownTime;
        private bool isTargetingCharacter = false;

        private void Start()
        {
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

            agent.updateRotation = false;
            agent.updatePosition = true;

            if (target == null)
                target = new GameObject(name + "_Target").transform;

            if (mainCam == null) mainCam = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                mouseDownTime = Time.time;
                HandleMouseClickLogic();
            }

            // Логика зажатия
            if (Input.GetMouseButton(1) && (Time.time - mouseDownTime > holdThreshold))
            {
                agent.speed = runSpeed;
                SetTargetFromMouse();
            }

            if (target != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                agent.SetDestination(target.position);

                // Проверка дистанции до персонажа через тег
                if (isTargetingCharacter)
                {
                    float distance = Vector3.Distance(transform.position, target.position);
                    if (distance <= agent.stoppingDistance + 0.2f)
                    {
                        StartScr();
                        isTargetingCharacter = false;
                    }
                }
            }

            // Передаем данные в скрипт анимации
            if (agent.remainingDistance > agent.stoppingDistance)
                character.Move(agent.desiredVelocity, false, false);
            else
                character.Move(Vector3.zero, false, false);
        }

        private void HandleMouseClickLogic()
        {
            float timeSinceLastClick = Time.time - lastClickTime;
            agent.speed = (timeSinceLastClick <= doubleClickThreshold) ? runSpeed : walkSpeed;
            lastClickTime = Time.time;
            SetTargetFromMouse();
        }

        private void SetTargetFromMouse()
        {
            if (mainCam == null) return;

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // ПРОВЕРКА ПО ТЕГАМ
                if (hit.collider.CompareTag("Ground"))
                {
                    isTargetingCharacter = false;
                    agent.stoppingDistance = 0.1f;
                    target.position = hit.point;
                }
                else if (hit.collider.CompareTag("Character"))
                {
                    isTargetingCharacter = true;
                    agent.stoppingDistance = 2.5f;
                    target.position = hit.collider.transform.position;

                    // Если кликнули, будучи уже рядом
                    if (Vector3.Distance(transform.position, target.position) <= 2.5f)
                    {
                        StartScr();
                        isTargetingCharacter = false;
                    }
                }
            }
        }

        public void StartScr()
        {
            Main_Flow.ExecuteBlock("test");
        }
        public Flowchart Main_Flow;
    }
}
