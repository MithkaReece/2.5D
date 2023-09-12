using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShadow : MonoBehaviour
{
    [SerializeField] PlayerJumping _playerJumping;

    Renderer _renderer;
    [SerializeField] float maxHeight;

    [SerializeField] float widthRatio;
    [SerializeField] float heightRatio;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = _playerJumping.GetShadowPosition();

        float playerHeightRatio = Mathf.Max(0, maxHeight - _playerJumping.GetHeight()) / maxHeight;        
        transform.localScale = new Vector3(widthRatio * playerHeightRatio, heightRatio * playerHeightRatio, transform.localScale.z);

        float opacity = playerHeightRatio;
        Color color = _renderer.material.color;
        color.a = opacity;
        _renderer.material.color = color;
    }
}
