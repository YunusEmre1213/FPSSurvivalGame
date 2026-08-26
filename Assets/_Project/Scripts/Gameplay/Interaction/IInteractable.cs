namespace Project.Gameplay.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }

        void Interact();
    }
}