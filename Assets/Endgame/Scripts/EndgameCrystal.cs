using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameCrystal : MidBattleScreen
{
    public List<EndgameHalfCrystal> Parts;
    public ParticleSystem IdleParticles;
    public ParticleSystem Explosion;
    private System.Action onFinish;

    private void Update()
    {
        if (Parts[0].Exploding && !Explosion.isPlaying)
        {
            Explosion.Play();
        }
        if (Parts[0] == null) // Bad check, but whatevs
        {
            MidBattleScreen.Set(this, false);
            onFinish?.Invoke();
            Destroy(gameObject);
        }
    }

    public void Shatter(System.Action onFinish)
    {
        this.onFinish = onFinish;
        Parts.ForEach(a => a.Explode());
        IdleParticles.Stop();
        MidBattleScreen.Set(this, true);
    }
}
