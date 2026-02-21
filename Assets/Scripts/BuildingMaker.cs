using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(SplineContainer))]
public class BuildingMaker : MonoBehaviour
{
    private SplineContainer _splineContainer;

    private float _distance;
    
    private List<Vector3> _points;
    private List<Vector3> _tangents;

    private void Awake()
    {
        _splineContainer = GetComponent<SplineContainer>();
    }

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
    }
    
    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }
    
    private void OnSplineChanged(Spline spline, int value, SplineModification modification)
    {
        if (spline != _splineContainer.Spline || modification != SplineModification.KnotModified)
        {
            return;
        }
        
        CalculatePoints();
    }

    private void CalculatePoints()
    {
        _points = new List<Vector3>();
        _tangents = new List<Vector3>();
        
        var spline = _splineContainer.Spline;
        
        _points.Add(spline.EvaluatePosition(0f));
        _tangents.Add(spline.EvaluateTangent(0f));

        if (_distance <= 0f)
        {
            return;
        }

        spline.GetPointAtLinearDistance(0f, _distance, out var t);
        while (t < 1f)
        {
            _points.Add(spline.EvaluatePosition(t));
            _tangents.Add(spline.EvaluateTangent(t));
            
            spline.GetPointAtLinearDistance(t, _distance, out t);
        }

    }

    private void OnDrawGizmosSelected()
    {
        if (_points == null || _tangents == null)
        {
            return;
        }
        
        for (var i = 0; i < _points.Count; i++)
        {
            var point = _points[i];
            var tangent = _tangents[i];
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.1f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(point, point + tangent.normalized);
        }
    }
}
