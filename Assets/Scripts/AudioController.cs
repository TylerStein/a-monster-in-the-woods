using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _ambientSource;
    [SerializeField] private AudioSource _baseWindSource;
    [SerializeField] private AudioSource _distortWindSource;

    [SerializeField] private AudioSource _chimeSource;
    [SerializeField] private Vector2 _chimePitchRange = Vector2.one;

    [SerializeField] private float _baseWindSourceVolume = 1f;
    [SerializeField] private float _distortWindSourceVolume = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _ambientSource.loop = true;
        _baseWindSource.loop = true;
        _distortWindSource.loop = true;
        _chimeSource.loop = false;

        _baseWindSource.volume = _baseWindSourceVolume;
        _distortWindSource.volume = 0f;

        _ambientSource.Play();
        _baseWindSource.Play();
        _distortWindSource.Play();
    }

    public void UseDistortedWind(int playerIndex) {
        _baseWindSource.volume = 0f;
        _distortWindSource.volume = _distortWindSourceVolume;
    }

    public void UseNormalWind(int playerIndex) {
        _baseWindSource.volume = _baseWindSourceVolume;
        _distortWindSource.volume = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (_chimeSource.isPlaying == false) {
            _chimeSource.loop = false;
            _chimeSource.pitch = Random.Range(_chimePitchRange.x, _chimePitchRange.y);
            _chimeSource.Play();
        }
    }
}
