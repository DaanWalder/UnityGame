using UnityEngine;

// Attach this class to the object that contains your worldbuilder
[RequireComponent(typeof(WorldBuilder))]
public  class AddCustomWorker : MonoBehaviour
{
    public void Start()
    {
        var worldBuilder = GetComponent<WorldBuilder>();

        var customWork = new MyCustomWorker();
        worldBuilder.AddCustomWorker(customWork);

        // Add as many custom workers as you like
        /* var customWork1 = new MyCustomWorker1();
           worldBuilder.AddCustomWorker(customWork1); */

        // You can also remove a worker if you want to change behaviour at some point
        // worldBuilder.RemoveCustomWorker(customWork);
        
        // You can add/remove custom workers at anytime if you want to change behaviour of your game
        // but not that only terrain generated after this point will be affected 
    }
}

