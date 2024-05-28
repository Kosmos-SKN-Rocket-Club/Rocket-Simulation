using System;
using System.IO;
using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Flight : MonoBehaviour
{
    public int targetFrameRate = 30;

    // File variables
    public string file = "flight0.txt";
    string path;
    string[] lines;
    string currentLine;
    string previousLine;
    string[] stringCurrentData = new string[7] {"", "", "", "", "", "", ""};
    string[] stringPreviousData = new string[7] {"", "", "", "", "", "", ""};
    double[] currentData = new double[7] {0, 1, 0, 0, 0, 0, 0};
    double[] previousData = new double[7] {0, 0, 0, 0, 0, 0, 0};
    long numberOfLines = 0;

    // Serial port variables
    public bool isRealTime = false;
    SerialPort stream;
    public string port = "COM4";
    public int baudrate = 9600;

    string portOutput;

    // Simulation variables
    int currentCordinate = 0;
    double rangeToTravel = 0;
    double rangeTraveled = 0;
    double currentVelocity = 0;
    double distance = 0;
    double avrVelocity = 0;
    double velocityIncrement = 0;
    double stepX = 0;
    double stepY = 0;
    double stepZ = 0;
    double numberOfSteps = 0;
    Quaternion rotated;
    void readFile()
    {
        lines = File.ReadAllLines(path);
        numberOfLines = lines.Length;
    }

    void saveToFile()
    {
        portOutput = stream.ReadLine();
        if(portOutput != null)
        {
            File.AppendAllText(path, portOutput);
        }
    }

    void extractDataForCalculations()
    {
        previousLine = lines[currentCordinate - 1];
        currentLine = lines[currentCordinate];
        stringPreviousData = previousLine.Split(' ');
        stringCurrentData = currentLine.Split(' ');
        for (int i = 0; i < stringCurrentData.Length; i++)
        {
            currentData[i] = Convert.ToDouble(stringCurrentData[i]);
            previousData[i] = Convert.ToDouble(stringPreviousData[i]);
        }
    }

    void calculateMovement()
    {
        rangeToTravel += (double) Math.Sqrt(Math.Pow((double)currentData[0] - previousData[0], 2)
                                            + Math.Pow((double)currentData[1] - previousData[1], 2)
                                            + Math.Pow((double)currentData[2] - previousData[2], 2));
        distance = rangeToTravel - rangeTraveled;
        avrVelocity = (Math.Abs((double)previousData[3]) + Math.Abs((double)currentData[3])) / 2;
        if((avrVelocity*1000000000) != 0)
        {
            numberOfSteps =  distance / avrVelocity * targetFrameRate;
            if(Math.Round(numberOfSteps*1000000000) != 0)
            {
                velocityIncrement = (double)((Math.Abs((double)currentData[3])
                                                - Math.Abs((double)previousData[3])) / numberOfSteps);
                stepX = (double)(((double)currentData[0] - previousData[0]) / numberOfSteps);
                stepY = (double)(((double)currentData[1] - previousData[1]) / numberOfSteps);
                stepZ = (double)(((double)currentData[2] - previousData[2]) / numberOfSteps);
                rotated = Quaternion.Euler((float)currentData[4], (float)currentData[5], (float)currentData[6]);
            }
        }
    }

     void moveRocket()
    {
        currentVelocity += velocityIncrement;
        transform.position += new Vector3((float)(stepX / avrVelocity * currentVelocity),
                                          (float)(stepY / avrVelocity * currentVelocity),
                                          (float)(stepZ / avrVelocity * currentVelocity));
        transform.rotation = Quaternion.Slerp(transform.rotation, rotated, Time.deltaTime * (float)(5 *avrVelocity / distance));
        rangeTraveled += (double) Math.Sqrt(Math.Pow(stepX, 2) + Math.Pow(stepY, 2) + Math.Pow(stepZ, 2));
    }

     void Start()
    {
        path = "..\\rocket simulation\\Assets\\Scripts\\" + file;
        stream = new SerialPort(port, baudrate);
        readFile();
        stream.Open();
    }

    void Update()
    {
        QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
        if(isRealTime)
        {
            saveToFile();
        }
        readFile();
        if (rangeTraveled >= rangeToTravel)
        {
            transform.position = new Vector3((float) currentData[0], (float) currentData[1], (float) currentData[2]);
            currentCordinate++;
            if(currentCordinate < numberOfLines)
            {
                extractDataForCalculations();
                calculateMovement();
            }
        }
        if(currentCordinate < numberOfLines && Math.Round(avrVelocity*1000000000) != 0 && Math.Round(numberOfSteps*1000000000) != 0)
        {
            moveRocket();
        }
    }
}
