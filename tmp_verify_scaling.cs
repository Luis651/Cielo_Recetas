using System;

public class ScalingTest
{
    public static void Main()
    {
        double baseSize = 22.0;
        double targetSize = 11.0;
        
        // La nueva lógica:
        double scalingFactor = targetSize / baseSize;
        
        Console.WriteLine($"Base: {baseSize}cm");
        Console.WriteLine($"Nuevo: {targetSize}cm");
        Console.WriteLine($"Factor esperado: 0.5");
        Console.WriteLine($"Factor calculado: {scalingFactor}");
        
        if (Math.Abs(scalingFactor - 0.5) < 0.001)
        {
            Console.WriteLine("VERIFICACIÓN EXITOSA: Escalado lineal correcto.");
        }
        else
        {
            Console.WriteLine("FALLO: El escalado no es lineal.");
        }
    }
}
