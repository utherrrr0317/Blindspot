﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isPlayed = false;
    private VideoPlayer videoPlayer;
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(videoPlayer.isPlaying && !isPlayed) {
            isPlayed = true;
        }
        if(isPlayed && !videoPlayer.isPlaying) {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<Chapter1Manager>().ChangeStage();
            gameObject.SetActive(false);
        }
    }

    private void OnEnable() {
        videoPlayer = GetComponent<VideoPlayer>();
    }
}