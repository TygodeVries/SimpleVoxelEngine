namespace Client.UserInterface;

public class GameInterface
{
    private static List<InterfaceElement> elements = new List<InterfaceElement>();

    public static void AddElement(InterfaceElement interfaceElement)
    {
        elements.Add(interfaceElement);
        interfaceElement.Create();
    }

    public static void RemoveElement(InterfaceElement interfaceElement)
    {
        elements.Remove(interfaceElement);
        interfaceElement.Destroy();
    }
}
