using UnityEngine;
using System.Collections.Generic;

namespace ImbroglioCombat.Utils
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public List<Sound> sounds = new List<Sound>();
        public List<Sound> music = new List<Sound>();

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        [Range(0f, 1f)]
        public float musicVolume = 1f;

        private Dictionary<string, Sound> soundDictionary;
        private Dictionary<string, Sound> musicDictionary;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            soundDictionary = new Dictionary<string, Sound>();
            musicDictionary = new Dictionary<string, Sound>();

            foreach (Sound sound in sounds)
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;

                soundDictionary[sound.name] = sound;
            }

            foreach (Sound song in music)
            {
                song.source = gameObject.AddComponent<AudioSource>();
                song.source.clip = song.clip;
                song.source.volume = song.volume;
                song.source.pitch = song.pitch;
                song.source.loop = song.loop;

                musicDictionary[song.name] = song;
            }
        }

        public void PlaySound(string name)
        {
            if (soundDictionary.ContainsKey(name))
            {
                Sound sound = soundDictionary[name];
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
                sound.source.Play();
            }
            else
            {
                Debug.LogWarning($"Sound {name} not found!");
            }
        }

        public void PlayMusic(string name)
        {
            if (musicDictionary.ContainsKey(name))
            {
                Sound song = musicDictionary[name];
                song.source.volume = song.volume * musicVolume * masterVolume;
                song.source.Play();
            }
            else
            {
                Debug.LogWarning($"Music {name} not found!");
            }
        }

        public void StopSound(string name)
        {
            if (soundDictionary.ContainsKey(name))
            {
                soundDictionary[name].source.Stop();
            }
        }

        public void StopMusic(string name)
        {
            if (musicDictionary.ContainsKey(name))
            {
                musicDictionary[name].source.Stop();
            }
        }

        public void StopAllMusic()
        {
            foreach (var song in musicDictionary.Values)
            {
                song.source.Stop();
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        void UpdateAllVolumes()
        {
            foreach (var sound in soundDictionary.Values)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
            }

            foreach (var song in musicDictionary.Values)
            {
                song.source.volume = song.volume * musicVolume * masterVolume;
            }
        }
    }
}

