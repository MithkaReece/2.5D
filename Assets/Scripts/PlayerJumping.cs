using UnityEngine;
using System.Collections;
using static Player;
using UnityEngine.UIElements;

public class PlayerJumping : MonoBehaviour
{
    [SerializeField] private float _jumpSpeed = 2f;
    [SerializeField] private float _airMovementSpeed;
    [SerializeField] private LayerMask _collisionLayerMask;

    private float _gravity = 9.8f;
    private float _jumpVelocity = 0f;
    private float _heightOffset = 0f;
    private bool _onGround = true;

    private float _currentHeight;
    private float _groundY;

    private int _groundLevel;

    private Player _player;
    [SerializeField] private PlayerAnimator _playerAnimator;

    Rigidbody2D _rb;
    BoxCollider2D _playerCollider;

    public Vector3 GetShadowPosition() {
        if (_onGround) return transform.position;
        return new Vector3(transform.position.x, _groundY, transform.position.z);
    }

    public float GetHeight() { return _currentHeight; }


    private void Awake()
    {
        _player = GetComponent<Player>();
        _rb = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
    }

    public void HandleJumping() {
        //Cause initial jump
        if (!GameInput.IsJumping() || _onGround == false) return;
        _player.SetState(PlayerState.Jumping);
        _currentReadyDuration = 0;

        Vector3Int groundPosition = Vector3Int.FloorToInt(new Vector3(transform.position.x, _groundY, transform.position.z));
        if (CalculateHeightMap.groundHeightMap.ContainsKey(groundPosition)) {
            _groundLevel = CalculateHeightMap.groundHeightMap[groundPosition];
        } else {
            Debug.Log("No ground: " + groundPosition); //TODO - Some ground has no level
        }
    }

    float _currentReadyDuration = 0f;

    public void Update() {

        //Vector3 movementVector = GameInput.GetMovementVector();

        //float rayDist = 3f;
        //Vector3 groundPosition = new Vector3(transform.position.x, _groundY, transform.position.z);

        //// Check for a collision with a wall along the movement direction using a raycast
        //RaycastHit2D hit = Physics2D.Raycast(groundPosition, movementVector, rayDist, _collisionLayerMask);

        //if (hit.collider != null) {
        //    Debug.Log("Collision");
        //} else Debug.Log("No Collision");

        if (_onGround)
            _groundY = transform.position.y;
    }

    public void ManualUpdate() {
        if (_onGround) {
            _currentReadyDuration += Time.deltaTime;

            if(_currentReadyDuration >= _playerAnimator.GetReadyDuration()) {
                _currentReadyDuration = 0f;
                // Actually jump
                _jumpVelocity = _jumpSpeed;
                _groundY = transform.position.y;
                _onGround = false;
                _rb.isKinematic = true;
            }
        } else {
            HandleGravity();
            HandleAirMovement();
        }
    }

    void HandleAirMovement() {
        // Move player based on input
        Vector3 movementVector = GameInput.GetMovementVector();

        float rayDist = 3f;
        Vector3 groundPosition = new Vector3(transform.position.x, _groundY, transform.position.z);

        // Check for a collision with a wall along the movement direction using a raycast
        RaycastHit2D hit = Physics2D.Raycast(groundPosition, movementVector, rayDist, _collisionLayerMask);

        if (hit.collider != null) {
            // Calculate the new position based on the input
            Vector3 newGroundPosition = groundPosition + rayDist * movementVector;

            //Get wall
            Vector3Int tilePosition = Vector3Int.FloorToInt(newGroundPosition);
            if (CalculateHeightMap.wallHeightMap.ContainsKey(tilePosition)) {
                float wallHeight = CalculateHeightMap.wallHeightMap[tilePosition];

                if (CalculateHeightMap.groundHeightMap.ContainsKey(Vector3Int.FloorToInt(groundPosition))) {
                    float groundLevel = CalculateHeightMap.groundHeightMap[Vector3Int.FloorToInt(groundPosition)];
                    Debug.Log("Check=>" + groundLevel + _currentHeight + "<" + wallHeight);
                    if (groundLevel + _currentHeight < wallHeight) return;
                    Debug.Log(groundLevel + _currentHeight + ">" + wallHeight);
                } else {
                    Debug.Log("No ground");
                }
            } else {
                Debug.Log("No wall");
            }
        }

        Vector3 movementOffset = movementVector * _airMovementSpeed * Time.deltaTime;
        transform.position += movementOffset;
        _groundY += movementOffset.y;

        //Calculate current ground
        CalculateGroundLevel(); 

        _player.HandleFacingDirection(movementVector);
    }
    //TODO: Needs to be fixed
    //Jumping a little bit doesn't not seem to trigger this so you land inside the wall
    void CalculateGroundLevel() {
        Vector3Int groundPosition = Vector3Int.FloorToInt(new Vector3(transform.position.x, _groundY, transform.position.z));
        if (CalculateHeightMap.groundHeightMap.ContainsKey(groundPosition)) {
            int groundLevel = CalculateHeightMap.groundHeightMap[groundPosition];
            if(groundLevel != _groundLevel) {
                Debug.Log("Changing ground level");
                int difference = groundLevel - _groundLevel;
                _groundLevel = groundLevel;
                // Add difference to ground to reach and height in air
                float scale = 0.5f;
                _groundY += scale*difference; 
                _currentHeight += scale*difference;
            }
        }
    }

    void HandleGravity() {
        if (_onGround) return;

        // Apply velocity to position
        _heightOffset += _jumpVelocity * Time.deltaTime;

        // Apply acceleration (gravity) to velocity
        _jumpVelocity -= _gravity * Time.deltaTime;

        // Update player position
        transform.position = new Vector3(transform.position.x,
            transform.position.y + _heightOffset,
            transform.position.z);

        //Track total height
        _currentHeight += _heightOffset;

        // Check for grounded (so far its just when you go back to previous y)
        if (_currentHeight <= 0) {
            _heightOffset = 0;
            _currentHeight = 0;

            transform.position = new Vector3(transform.position.x,
            _groundY, transform.position.z);

            _onGround = true;
            _rb.isKinematic = false;
            _player.SetState(PlayerState.Idle);
        }
    }
}

