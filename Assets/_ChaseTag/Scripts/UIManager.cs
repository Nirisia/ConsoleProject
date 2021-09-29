using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private string txtAnimPlay = "FadeOut";
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();    
        }

        public void BtnPlay()
        {
            animator.SetTrigger(txtAnimPlay);
        }
    }
}
