using UnityEngine;

namespace SkyClerik
{
    public class CubeInFixedUpfateAutoRotate : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _rotate = new Vector3(1, 0, 0);

        private void Update()
        {
        }

        private void FixedUpdate()
        {

            this.transform.Rotate(_rotate);
        }
    }
}
