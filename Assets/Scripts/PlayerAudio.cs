using UnityEngine;

public class PlayerAudio : MonoBehaviour {

    void Start() {
        PlayerMovement.Instance.OnJump += PlayerMovement_OnJump;
        PlayerMovement.Instance.OnAirJump += PlayerMovement_OnAirJump;
        PlayerMovement.Instance.OnWallJump += PlayerMovement_OnWallJump;
        PlayerMovement.Instance.OnDash += PlayerMovement_OnDash;
        PlayerShoot.Instance.OnShoot += PlayerShoot_OnShoot;
        GameManager.Instance.OnPowerUpReady += GameManager_OnPowerUpReady;
        HoldToPowerUp.Instance.OnPowerUpStart += HoldToPowerUp_OnPowerUpStart;
        HoldToPowerUp.Instance.OnPowerUpEnd += HoldToPowerUp_OnPowerUpEnd;
        PlayerHealth.Instance.OnGetHit += PlayerHealth_OnGetHit;
        PlayerHealth.Instance.OnPlayerDie += PlayerHealth_OnPlayerDie;
        Star.OnStarCollect += Star_OnStarCollect;
        HealthItem.OnHealthCollect += HealthItem_OnHealthCollect;
        WizardHat.OnWizardHatCollect += WizardHat_OnWizardHatCollect;
        BounceTrap.BounceTrapTrigger += BounceTrapTrigger;
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

    private void GameManager_OnPowerUpReady(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PowerUpReady");
    }

    private void HoldToPowerUp_OnPowerUpStart(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PowerUpStart");
    }

    private void HoldToPowerUp_OnPowerUpEnd(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PowerUpEnd");
    }

    private void PlayerHealth_OnGetHit(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerGetHit");
    }

    private void PlayerHealth_OnPlayerDie(object sender, System.EventArgs e) {
        SoundEffectManager.Instance.Play("PlayerDie");
    }

    private void Star_OnStarCollect(int starValue) {
        SoundEffectManager.Instance.Play("StarCollect");
    }

    private void HealthItem_OnHealthCollect(int healthIncrease, int maxHealthIncrease) {
        SoundEffectManager.Instance.Play("HealthItemCollect");
    }

    private void WizardHat_OnWizardHatCollect(float bulletSpeedIncrease, float fireRateIncrease, int bulletDamageIncrease) {
        SoundEffectManager.Instance.Play("WizardHatCollect");
    }

    private void BounceTrapTrigger() {
        SoundEffectManager.Instance.Play("BounceTrapTrigger");
    }
}
