using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour {
    [SerializeField] float fallWait = 1f;
    [SerializeField] float destroyWait = 2f;

    private bool isFalling;
    private Rigidbody2D myRigidBody;

    private void Awake() {
        myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out PlayerMovement player) && !isFalling) {
            StartCoroutine(Fall());
        }
    }

    private IEnumerator Fall() {
        isFalling = true;
        yield return new WaitForSeconds(fallWait);
        myRigidBody.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject, destroyWait);
    }
}
