using UnityEngine;

public class GameAudio : MonoBehaviour {
    private void Start() {
        GameManager.Instance.OnPowerUpReady += GameManager_OnPowerUpReady;
        HoldToPowerUp.Instance.OnPowerUpStart += HoldToPowerUp_OnPowerUpStart;
        HoldToPowerUp.Instance.OnPowerUpEnd += HoldToPowerUp_OnPowerUpEnd;
        Star.OnStarCollect += Star_OnStarCollect;
        HealthItem.OnHealthCollect += HealthItem_OnHealthCollect;
        WizardHat.OnWizardHatCollect += WizardHat_OnWizardHatCollect;
        BounceTrap.BounceTrapTrigger += BounceTrapTrigger;
        SpikeTrap.SpikeTrapTrigger += SpikeTrapTrigger;
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

    private void SpikeTrapTrigger() {
        SoundEffectManager.Instance.Play("SpikeTrapTrigger");
    }
}
