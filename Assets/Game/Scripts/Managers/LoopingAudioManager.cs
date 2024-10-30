using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingAudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private float _defaultVolume = 0.5f;
    [SerializeField] private GameState _gameState;
    

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        StartCoroutine(PlayRandomClip());
    }

    void Update()
    {
        if ( _gameState.HasStarted && _audioSource.volume > 0){
            StartCoroutine(GradualMute());
        }
        else if (!_gameState.HasStarted && _audioSource.volume == 0){
            StartCoroutine(PlayRandomClip());
        }
    }

    private IEnumerator PlayRandomClip()
    {
        _audioSource.volume = _defaultVolume;
        _audioSource.clip = _audioClips[Random.Range(0, _audioClips.Length)];
        _audioSource.Play();
        yield return new WaitForSeconds(_audioSource.clip.length);
        if (_audioSource.volume > 0){
            StartCoroutine(PlayRandomClip());
        }
    }

    public IEnumerator GradualMute(){
        Debug.Log("Muting Looping Audio");
        while(_audioSource.volume > 0){
            _audioSource.volume -= 0.0001f;
            yield return new WaitForSeconds(0.2f);
        }

    }
}
