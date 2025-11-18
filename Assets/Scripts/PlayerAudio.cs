using UnityEngine;
using System.Collections;

public class PlayerAudio : MonoBehaviour {

    void Start() {
        PlayerMovement.Instance.OnJump += PlayerMovement_OnJump;
        PlayerMovement.Instance.OnAirJump += PlayerMovement_OnAirJump;
        PlayerMovement.Instance.OnWallJump += PlayerMovement_OnWallJump;
        PlayerMovement.Instance.OnDash += PlayerMovement_OnDash;
        PlayerShoot.Instance.OnShoot += PlayerShoot_OnShoot;
        PlayerHealth.Instance.OnGetHit += PlayerHealth_OnGetHit;
        PlayerHealth.Instance.OnPlayerDie += PlayerHealth_OnPlayerDie;
    }

    private void PlayerMovement_OnJump(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerJump");
    }

    private void PlayerMovement_OnAirJump(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerAirJump");
    }

    private void PlayerMovement_OnWallJump(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerWallJump");
    }

    private void PlayerMovement_OnDash(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerDash");
    }

    private void PlayerShoot_OnShoot(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerShoot");
    }

    private void PlayerHealth_OnGetHit(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerGetHit");
    }

    private void PlayerHealth_OnPlayerDie(object sender, System.EventArgs e) {
        StartCoroutine(PlayerDied());
    }

    private IEnumerator PlayerDied() {
        SoundEffectManager.Instance.Play("PlayerDie");
        yield return new WaitForSeconds(5f);
        this.enabled = false;
    }

    void OnDisable() {
        PlayerMovement.Instance.OnJump -= PlayerMovement_OnJump;
        PlayerMovement.Instance.OnAirJump -= PlayerMovement_OnAirJump;
        PlayerMovement.Instance.OnWallJump -= PlayerMovement_OnWallJump;
        PlayerMovement.Instance.OnDash -= PlayerMovement_OnDash;
        PlayerShoot.Instance.OnShoot -= PlayerShoot_OnShoot;
        PlayerHealth.Instance.OnGetHit -= PlayerHealth_OnGetHit;
        PlayerHealth.Instance.OnPlayerDie -= PlayerHealth_OnPlayerDie;
    }
}
