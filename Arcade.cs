using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports; //Tutorial para tontos: Herramientas -> Administrador de paquetes NuGet -> Administrar paquetes de NuGet para la solucion
                       // Buscar o Ctrl + L -> System.io.ports -> INSTALAR Y REINICIAR VISUAL STUDIO COMUNITY
using System;
using System.Linq;

public class Arcade : MonoBehaviour
{
    public static Arcade ac;
    SerialPort serialPort;
    private string[] keys = { "la", "ra", "lb", "rb", "l1", "r1", "l2", "r2", "j1_Up", "j2_Up", "j1_Down", "j2_Down", "j1_Left", "j2_Left", "j1_Right", "j2_Right" };
    private Vector2 j1 = new Vector2(0, 0);
    private Vector2 j2 = new Vector2(0, 0);
    private Dictionary<string, bool> keyStates = new Dictionary<string, bool>();
    private Dictionary<string, bool> keyDown = new Dictionary<string, bool>();
    private Dictionary<string, bool> keyUp = new Dictionary<string, bool>();
    private Dictionary<string, byte> j1D = new Dictionary<string, byte>() { { "Up", 0 }, { "Down", 0 }, {"Left", 0}, { "Right", 0 } };
    private Dictionary<string, byte> j2D = new Dictionary<string, byte>() { { "Up", 0 }, { "Down", 0 }, { "Left", 0 }, { "Right", 0 } };
    void Awake()
    {
        if (ac != null && ac != this)
            Destroy(this);
        else
            ac = this;
        serialPort = StartArduino();
        if (serialPort != null)
        {
            serialPort.Open();
            serialPort.ReadTimeout = 50;
        }
        foreach (string key in keys)
        {
            keyStates[key] = false;
            keyDown[key] = false;
            keyUp[key] = false;
        }
    }
    SerialPort StartArduino()
    {
        string[] ports = SerialPort.GetPortNames();

        foreach (string port in ports)
        {
            SerialPort testPort = new SerialPort(port, 9600);
            try
            {
                testPort.Open();
                if (testPort.IsOpen)
                {
                    Debug.Log("Arduino conectado en " + port);
                    testPort.Close();
                    serialPort = testPort;
                    return testPort;
                }
            }
            catch (System.Exception)
            {
                // Si hay algún error al abrir el puerto, simplemente continuamos con el siguiente
                continue;
            }
        }
        
        Debug.LogWarning("Arduino no encontrado en ningún puerto COM");
        return null;
    }
    private void Update()
    {
        if (serialPort != null)
        {
            if (serialPort.IsOpen)
            {
                resetInputs();
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SendDataToArduino(243, "rojo", "azul");
                }
                try
                {
                    string[] parDiv = serialPort.ReadLine().Split(":"); //INFORMACION LLEGA "ra:false" - "j1_left:false"
                    if (keys.Contains(parDiv[0]))
                    {
                        bool entrada = bool.Parse(parDiv[1]);
                        if (keyStates[parDiv[0]] != entrada)
                        {
                            if (entrada)
                            {
                                keyDown[parDiv[0]] = true;
                            }
                            else
                            {
                                keyUp[parDiv[0]] = true;
                            }
                        }
                        keyStates[parDiv[0]] = entrada;
                    }
                    else if (parDiv[0] == "j1" || parDiv[0] == "j2")//j1_Up:false
                    {
                        int h = 0;
                        int v = 0;
                        string[] jostickSplit = parDiv[0].Split("_");
                        bool res = bool.Parse(parDiv[1]);
                        if (res)
                        {
                            switch (jostickSplit[1])
                            {
                                case "Down":
                                    v = -1;
                                    break;
                                case "Up":
                                    v = 1;
                                    break;
                                case "Right":
                                    h = 1;
                                    break;
                                case "Left":
                                    h = -1; break;
                            }
                        }

                        if (parDiv[0] == "j1")
                            j1 = new Vector2(h, v);
                        else
                            j2 = new Vector2(h, v);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                serialPort.Close();
                Debug.Log("Se cerro la conexion");
            }
        }
    }
    private void resetInputs()
    {
        /// "LA" - "RA" - "LB" - "RB" - "L1" - "R2" - "j1_Up" - "j2_Down
        foreach (string key in keys)
        {
            keyDown[key] = false;
            keyUp[key] = false;
        }
    }
    /// <summary>
    /// Funcion que devuelve el valor de un boton
    /// "LA" - "RA" - "LB" - "RB" - "L1" - "R2" - "j1_Up" - "j2_Down
    /// </summary>
    public bool Button(string key)
    {
        return keyStates[key];
    }
    /// <summary>
    /// Funcion que devuelve cuando el boton se presiona por primera vez
    /// "LA" - "RA" - "LB" - "RB" - "L1" - "R2" - "j1_Up" - "j2_Down
    /// </summary>
    public bool ButtonDown(string key)
    {
        return keyDown[key];
    }
    /// <summary>
    /// Funcion que devuelve cuando el boton se suelta
    /// "LA" - "RA" - "LB" - "RB" - "L1" - "R2" - "j1_Up" - "j2_Down"
    /// </summary>
    public bool ButtonUp(string key)
    {
        return keyUp[key];
    }
    /// <summary>
    /// Funcion que devuelve un Vector(x,y) entre -1 y 1
    /// </summary>
    /// <param name="key"></param>
    /// <returns>Vector 2</returns>
    public Vector2 Joystick(string key)//JOySTICK pa los especialitos
    {
        if (key == "j1")
            return j1;
        else if (key == "j2")
            return j2;
        else
            return new Vector2(0, 0);
    }
    void SendDataToArduino(int porcentaje, string texto1, string texto2)
    {
        string message = porcentaje.ToString() + "," + texto1 + "," + texto2 + "\n";

        serialPort.Write(message);
    }
    void OnDestroy()
    {
        serialPort.Close();
    }
}