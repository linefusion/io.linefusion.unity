using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyProject.Managers
{
    public partial class AudioManager : MonoBehaviour
    {
        private void OnEnable()
        {
            this.RegisterEventListeners();
        }

        private void OnDisable()
        {
            this.UnregisterEventListeners();
        }
        
        public void Play(AudioSource source, Vector3 position)
        {
            var obj = Instantiate(source.gameObject, this.transform);
            var audio = obj.GetComponent<AudioSource>();
            obj.transform.position = position;
            audio.Play();
            Destroy(obj, audio.clip.length);
        }

        public void Play(AudioSource source)
        {
            var obj = Instantiate(source.gameObject, this.transform);
            var audio = obj.GetComponent<AudioSource>();
            audio.Play();
            Destroy(obj, audio.clip.length);
        }
    }
}
