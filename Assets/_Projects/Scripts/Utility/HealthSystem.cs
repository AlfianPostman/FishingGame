
public class HealthSystem
{
    public float maxHealth { get; private set;}
    public float health { get; private set;}

    public HealthSystem(int maxHealth) 
    {
        this.maxHealth = maxHealth;
        health = this.maxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)health / maxHealth;        
    }

    public void Damage(float amount)
    {
        health -= amount;
        if(health < 0) health = 0;
    }

    public void Heal(float amount)
    {
        health += amount;
        if(health > maxHealth) health = maxHealth;
    }
}
