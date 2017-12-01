using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Geometry;
using UnityEngine.EventSystems;
using Assets;
using System;
using Assets.Scripts.Metrics;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(MeshFilter))]
public class AircraftMesh : MonoBehaviour
{

    public float rotationSpeed = 8;

    public Aircraft aircraft = new Aircraft();
    public Camera cam;
    public EventSystem eventSys;

    public Text weightText;
    public Text passengerText;
    public Text C_D0Text;
    public Text rangeText;
    public Text dCmdClText;
    public Text staticMarginText;
    public Text landingGearText;
    public Text trimmabilityText;
    public Text maxSpeedText;
    public Text takeoffGroundRollText;
    public Text landingGroundRollText;
    public Text FAR25Text;
    public Text ErrorText;
    public Canvas UI;
    Mesh mesh;

    private Animation animation;
    private Vector3 offset;
    private Vector3 original;
    private Quaternion originalRotation;
    

    private float timeSinceUpdate;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        UpdateMesh();
        eventSys = GameObject.Find("EventSystem").GetComponent<EventSystem>();

        SetNoseLengthDiameterRatio(Constants.Defaults.Fuselage.NoseLengthDiameterRatio);

        animation = GetComponent<Animation>();

        offset = transform.position - cam.transform.position;

        cam.transform.LookAt(new Vector3(0, 0, 600));
        original = transform.position;
        originalRotation = transform.rotation;

        CalculateMetrics();
    }



    // Update is called once per frame
    void Update()
    {

        timeSinceUpdate += Time.deltaTime;

        if (timeSinceUpdate > 0.5f && Input.touchCount == 0 && !Input.GetMouseButton(0) && !animation.isPlaying)
        {
            CalculateMetrics();
            timeSinceUpdate = 0;
        }

        if (Input.GetMouseButton(0) && !eventSys.IsPointerOverGameObject())
        {
            cam.transform.RotateAround(new Vector3(0, 0, 600), Vector3.up, (Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime));
            cam.transform.RotateAround(new Vector3(0, 0, 600), cam.transform.right, (Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime));
        }

        if (Input.touchCount > 0 && !eventSys.IsPointerOverGameObject())
        {
            cam.transform.RotateAround(new Vector3(0, 0, 600), Vector3.up, (Input.touches[0].deltaPosition.x) * -rotationSpeed / 4 * Time.deltaTime);
            cam.transform.RotateAround(new Vector3(0, 0, 600), cam.transform.right, (Input.touches[0].deltaPosition.y) * -rotationSpeed / 4 * Time.deltaTime);
        }

        if (animation.isPlaying && !animation.IsPlaying("cameraRoll"))
        {
            cam.transform.position = transform.position - offset;
        }
        else
        {
            transform.position = new Vector3(original.x, transform.position.y, original.z);
            transform.rotation = originalRotation;
            //cam.transform.position = original - offset;
        }

        if(animation.isPlaying)
        {
            UI.enabled = false;
        }
        else
        {
            UI.enabled = true;
        }

    }

    void UpdateMesh()
    {
        mesh.Clear();

        aircraft.Generate(mesh);

        mesh.RecalculateNormals();
    }

    // Editing geometry

    public void SetNoseLengthDiameterRatio(float value)
    {
        //aircraft.Fuselage.Nose.NoseLengthDiameterRatio = value;
        //aircraft.Fuselage.UpdateSections();

        //UpdateMesh();

        aircraft.Fuselage.NoseLengthDiameterRatio = value;

        transform.Find("Nose").localScale = new Vector3(aircraft.Fuselage.Diameter / 200, aircraft.Fuselage.Diameter / 200, aircraft.Fuselage.Diameter * value / 200);
    }

    public void SetSeatsAcross(float value)
    {
        var seatsAcross = Convert.ToInt32(value);
        var diameter = AircraftStructures.SeatsToDiameter(seatsAcross);

        aircraft.SeatsAcross = seatsAcross;

        aircraft.Fuselage.Diameter = diameter;
        aircraft.Wing.FuselageDiameter = diameter;
        aircraft.HorizontalStabilizer.FuselageDiameter = diameter;
        aircraft.VerticalStabilizer.FuselageDiameter = diameter;
        SetNoseLengthDiameterRatio(aircraft.Fuselage.NoseLengthDiameterRatio);

        UpdateMesh();
    }

    public void SetCentreFuselageLength(float value)
    {
        aircraft.Fuselage.CentreFuselage.Length = value;
        aircraft.Wing.CentreFuselageLength = value;
        aircraft.HorizontalStabilizer.CentreFuselageLength = value;
        aircraft.VerticalStabilizer.CentreFuselageLength = value;
        UpdateMesh();
    }

    public void SetAfterbodyLengthDiameterRatio(float value)
    {
        aircraft.Fuselage.Afterbody.AfterbodyLengthDiameterRatio = value;
        aircraft.HorizontalStabilizer.AfterbodyLengthDiameterRatio = value;
        aircraft.VerticalStabilizer.AfterbodyLengthDiameterRatio = value;

        UpdateMesh();
    }

    // Wing
    public void SetWingSweep(float value)
    {
        aircraft.Wing.Sweep = value;

        UpdateMesh();
    }

    public void SetWingTaperRatio(float value)
    {
        aircraft.Wing.TaperRatio = value;

        UpdateMesh();
    }

    public void SetWingArea(float value)
    {
        aircraft.Wing.Area = value;
        aircraft.HorizontalStabilizer.Area = value * aircraft.HorizontalStabilizer.AreaRatio;

        UpdateMesh();
    }

    public void SetWingAspectRatio(float value)
    {
        aircraft.Wing.AspectRatio = value;

        UpdateMesh();
    }

    public void SetWingXPercentage(float value)
    {
        aircraft.Wing.XPercentage = value;
        UpdateMesh();
    }

    public void SetEngineBypassRatio(float value)
    {
        aircraft.Wing.EngineBypassRatio = value;

        UpdateMesh();
    }

    public void SetEnginesPerWing(float value)
    {
        aircraft.Wing.EnginesPerWing = Convert.ToInt32(value);

        UpdateMesh();
    }

    public void SetEngineDesignThrust(float value)
    {
        aircraft.Wing.EngineDesignThrust = value;

        UpdateMesh();
    }

    public void SetFlapExtensionRatio(float value)
    {
        aircraft.Wing.Flaps.ExtensionRatio = value;
    }

    public void SetFlapType(int value)
    {
        aircraft.Wing.Flaps.HighLiftDeviceType = (HighLiftDeviceType)value;
    }

    public void SetTailAreaRatio(float value)
    {
        aircraft.HorizontalStabilizer.Area = value * aircraft.Wing.Area;

        UpdateMesh();
    }

    // Metrics

    public void CalculateMetrics()
    {
        ErrorText.text = string.Empty;

        weightText.text = String.Format("{0:0.0} tons", (aircraft.CalculateWeight() / 1000)); ;
        passengerText.text = (aircraft.CalculateSeats()) + " passengers";

        C_D0Text.text = "C_D0" + (aircraft.CalculateC_D0(260, 11000));

        var cg = new Vector3(0, 0, -aircraft.CalculateCentreOfMass().z);

        for (int i = 0; i < 20; i++)
        {
            aircraft.AlignLandingGear(transform);
            aircraft.UpdateTailArea();
            aircraft.CalculateWeight();
        }

        var altitude = 11000f;
        var velocity = 260f;

        aircraft.Stats = new AircraftStats
        {
            Range = aircraft.CalculateRange(velocity, altitude) / 1000,
            StaticMargin = aircraft.CalculateStaticMargin(),
            LandingGearAlignmentStatus = aircraft.AlignLandingGear(transform),
            TakeoffDistance = aircraft.CalculateTakeoffGroundRoll(),
            LandingDistance = aircraft.CalculateLandingGroundRoll(),
            Trimmability = aircraft.EvaluateTrimmability(velocity, altitude),
        };

        rangeText.text = string.Format("Range: {0:0.0} km", aircraft.Stats.Range);
        dCmdClText.text = string.Format("dCmdCl: {0:0.00}", aircraft.EvaluateStability(velocity, altitude));
        staticMarginText.text = string.Format("Static Margin: {0:0.0}", aircraft.Stats.StaticMargin);
        landingGearText.text = string.Format("Landing gear: {0}", aircraft.Stats.LandingGearAlignmentStatus);
        trimmabilityText.text = string.Format("Trimmability: {0}", aircraft.Stats.Trimmability);
        maxSpeedText.text = string.Format("Max Speed at Cruise: {0:0.0}", aircraft.CalculateMaxSpeed(altitude));

        takeoffGroundRollText.text = string.Format("Takeoff Roll Distance: {0:0.00}m", aircraft.Stats.TakeoffDistance);
        landingGroundRollText.text = string.Format("Landing Roll Distance: {0:0.00}m", aircraft.Stats.LandingDistance);
        FAR25Text.text = string.Format("FAR25: {0}", aircraft.EvaluateFAR25().ToString());

        transform.Find("CentreOfGravity").localPosition = cg;

        UpdateMesh();

        if (aircraft.Stats.Trimmability == Trimmability.LiftNotAdequate || aircraft.Stats.TakeoffDistance > 3900f)
        {
            ErrorText.text += Constants.ErrorMessages.LiftNotAdequate + Environment.NewLine + Environment.NewLine;
        }

        if (aircraft.Stats.Trimmability == Trimmability.RotationallyUnstable || aircraft.Stats.StaticMargin < 0)
        {
            ErrorText.text += Constants.ErrorMessages.Unstable + Environment.NewLine + Environment.NewLine;

        }

        if (aircraft.Stats.LandingGearAlignmentStatus != LandingGearAlignmentStatus.Success)
        {
            ErrorText.text += Constants.ErrorMessages.TipBack + Environment.NewLine + Environment.NewLine;

        }
    }

    public void PlayCutscene()
    {
        UpdateMesh();

        if (aircraft.Stats.Trimmability == Trimmability.LiftNotAdequate || aircraft.Stats.TakeoffDistance > 3900f)
        {
            animation.Play("LiftNotAdequate");
        }
        else if (aircraft.Stats.Trimmability == Trimmability.RotationallyUnstable || aircraft.Stats.StaticMargin < 0)
        {
            animation.Play("Takeoff");
            animation.PlayQueued("Unstable");
        }
        else if (aircraft.Stats.LandingGearAlignmentStatus != LandingGearAlignmentStatus.Success)
        {
            animation.Play("Tipback");
        }
        else
        {
            animation.Play("Takeoff");
        }
    }

    public void SetFuelVolume(float value)
    {
        aircraft.Wing.Fuel.Volume = value;
    }
}

