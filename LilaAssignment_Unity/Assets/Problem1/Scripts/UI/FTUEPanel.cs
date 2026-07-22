using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    [RequireComponent(typeof(Animator))]
    public class FTUEPanel : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] Animator mAnimator;
        [SerializeField] Text _descriptionText;

        protected virtual void Awake()
        {
            mAnimator = this.gameObject.GetComponent<Animator>();
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {
            
        }

        public virtual void TriggerAnimation(string triggerName)
        {
            ToggleActive(true);
            mAnimator.SetTrigger(triggerName);
        }

        public virtual void TriggerExit()
        {
            ToggleActive(true);
            mAnimator.SetTrigger("Exit");
        }

        public void ToggleActive(bool isActive)
        {
            this.gameObject.SetActive(isActive);
        }

        public virtual void EnableText()
        {
            _descriptionText.gameObject.SetActive(true);
        }

        public virtual void DisableText()
        {
            _descriptionText.gameObject.SetActive(false);
        }
    }
}
