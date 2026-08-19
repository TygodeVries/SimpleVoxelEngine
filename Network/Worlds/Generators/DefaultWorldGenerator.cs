using System.Numerics;

namespace Shared.Worlds;

public class DefaultWorldGenerator : WorldGenerator
{
    public override short Generate(int x, int y, int z)
    {
        if (y > 0)
            return 0;
        Vector3 worldCenter = new Vector3(0, 0, 0);
        Vector3 point = new Vector3(x, y, z);

        if (Vector3.Distance(worldCenter, point) < 4)
            return 1;


        return 0;
    }
}
