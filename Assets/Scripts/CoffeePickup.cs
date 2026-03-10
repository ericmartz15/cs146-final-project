// CoffeePickup.cs
// Place this on a Coffee GameObject in the scene.
// When the player rides over it, they get a temporary speed boost.

using System.Collections;
using UnityEngine;

public class CoffeePickup : MonoBehaviour
{
    [Header("Speed Boost")]
    [SerializeField] private float speedMultiplier = 2f;   // How much faster the player goes
    [SerializeField] private float boostDuration = 3f;     // How long the boost lasts in seconds

    private void OnTriggerEnter2D(Collider2D other)
    {
        BikeController bike = other.GetComponent<BikeController>();
        if (bike != null)
        {
            bike.StartSpeedBoost(speedMultiplier, boostDuration);
            gameObject.SetActive(false); // Hide the coffee immediately on pickup
        }
    }
}