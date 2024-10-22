﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Prowl.Runtime.NodeSystem;

[Serializable]
public class NodePort
{
    public enum IO { Input, Output }

    public int ConnectionCount { get { return _connections.Count; } }
    /// <summary> Return the first non-null connection </summary>
    public NodePort Connection
    {
        get
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i] != null) return _connections[i].Port;
            }
            return null;
        }
    }

    public Node? ConnectedNode => Connection?.node;

    public IO direction
    {
        get { return _direction; }
        internal set { _direction = value; }
    }
    public Node.ConnectionType connectionType
    {
        get { return _connectionType; }
        internal set { _connectionType = value; }
    }
    public Node.TypeConstraint typeConstraint
    {
        get { return _typeConstraint; }
        internal set { _typeConstraint = value; }
    }

    /// <summary> Is this port connected to anything? </summary>
    public bool IsConnected { get { return _connections.Count != 0; } }
    public bool IsInput { get { return direction == IO.Input; } }
    public bool IsOutput { get { return direction == IO.Output; } }

    public string fieldName { get { return _fieldName; } }
    public Node node { get { return _node; } }
    public bool IsDynamic { get { return _dynamic; } }
    public bool IsStatic { get { return !_dynamic; } }
    public bool IsOnHeader { get { return _onHeader; } }
    public Type ValueType
    {
        get
        {
            if (_valueType == null && !string.IsNullOrEmpty(_typeQualifiedName)) _valueType = Type.GetType(_typeQualifiedName, false);
            return _valueType;
        }
        set
        {
            if (_valueType == value) return;
            _valueType = value;
            if (value != null) _typeQualifiedName = value.AssemblyQualifiedName;
        }
    }
    private Type _valueType;

    public Vector2 LastKnownPosition { get { return _lastKnownPosition; } set { _lastKnownPosition = value; } }

    [SerializeIgnore] private Vector2 _lastKnownPosition;

    [SerializeField] private string _fieldName;
    [SerializeField] private Node _node;
    [SerializeField] private string _typeQualifiedName;
    [SerializeField] private List<PortConnection> _connections = [];
    [SerializeField] private IO _direction;
    [SerializeField] private Node.ConnectionType _connectionType;
    [SerializeField] private Node.TypeConstraint _typeConstraint;
    [SerializeField] private bool _dynamic;
    [SerializeField] private bool _onHeader;
    [SerializeField] public int InstanceID = 0;

    public NodePort() { } // For Serialization

    /// <summary> Construct a static targetless nodeport. Used as a template. </summary>
    public NodePort(FieldInfo fieldInfo, Node? node)
    {
        _fieldName = fieldInfo.Name;
        ValueType = fieldInfo.FieldType;
        _dynamic = false;
        var attribs = fieldInfo.GetCustomAttributes(false);
        for (int i = 0; i < attribs.Length; i++)
        {
            if (attribs[i] is Node.InputAttribute)
            {
                _direction = IO.Input;
                _connectionType = (attribs[i] as Node.InputAttribute).connectionType;
                _onHeader = (attribs[i] as Node.InputAttribute).onHeader;
                _typeConstraint = (attribs[i] as Node.InputAttribute).typeConstraint;
            }
            else if (attribs[i] is Node.OutputAttribute)
            {
                _direction = IO.Output;
                _connectionType = (attribs[i] as Node.OutputAttribute).connectionType;
                _onHeader = (attribs[i] as Node.OutputAttribute).onHeader;
                _typeConstraint = (attribs[i] as Node.OutputAttribute).typeConstraint;
            }
            // Override ValueType of the Port
            if (attribs[i] is PortTypeOverrideAttribute)
            {
                ValueType = (attribs[i] as PortTypeOverrideAttribute).type;
            }
        }
        _node = node;
        InstanceID = node?.graph.NextID ?? 0;
    }

    /// <summary> Copy a nodePort but assign it to another node. </summary>
    public NodePort(NodePort nodePort, Node node)
    {
        _fieldName = nodePort._fieldName;
        ValueType = nodePort._valueType;
        _direction = nodePort.direction;
        _dynamic = nodePort._dynamic;
        _onHeader = nodePort._onHeader;
        _connectionType = nodePort._connectionType;
        _typeConstraint = nodePort._typeConstraint;
        _node = node;
        InstanceID = _node.graph.NextID;
    }

    /// <summary> Construct a dynamic port. Dynamic ports are not forgotten on reimport, and is ideal for runtime-created ports. </summary>
    public NodePort(string fieldName, Type type, IO direction, Node.ConnectionType connectionType, Node.TypeConstraint typeConstraint, Node node, bool onHeader)
    {
        _fieldName = fieldName;
        ValueType = type;
        _direction = direction;
        _node = node;
        _dynamic = true;
        _onHeader = onHeader;
        _connectionType = connectionType;
        _typeConstraint = typeConstraint;
        InstanceID = _node.graph.NextID;
    }

    /// <summary> Checks all connections for invalid references, and removes them. </summary>
    public void VerifyConnections()
    {
        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            if (_connections[i].node != null &&
                !string.IsNullOrEmpty(_connections[i].fieldName) &&
                _connections[i].node.GetPort(_connections[i].fieldName) != null)
                continue;
            _connections.RemoveAt(i);
        }
    }

    /// <summary> Return the output value of this node through its parent nodes GetValue override method. </summary>
    /// <returns> <see cref="Node.GetValue(NodePort)"/> </returns>
    public object GetOutputValue()
    {
        if (direction == IO.Input) return null;
        try
        {
            return node.GetValue(this);
        }
        catch (Exception e)
        {
            node.Error = "Error: " + e.Message;
            return null;
        }
    }

    /// <summary> Return the output value of the first connected port. Returns null if none found or invalid.</summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public object GetInputValue()
    {
        NodePort connectedPort = Connection;
        if (connectedPort == null) return null;
        return connectedPort.GetOutputValue();
    }

    /// <summary> Return the output values of all connected ports. </summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public object[] GetInputValues()
    {
        object[] objs = new object[ConnectionCount];
        for (int i = 0; i < ConnectionCount; i++)
        {
            NodePort connectedPort = _connections[i].Port;
            if (connectedPort == null)
            { // if we happen to find a null port, remove it and look again
                _connections.RemoveAt(i);
                i--;
                continue;
            }
            objs[i] = connectedPort.GetOutputValue();
        }
        return objs;
    }

    /// <summary> Return the output value of the first connected port. Returns null if none found or invalid. </summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public T GetInputValue<T>()
    {
        object obj = GetInputValue();
        return obj is T t ? t : default;
    }

    /// <summary> Return the output values of all connected ports. </summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public T[] GetInputValues<T>()
    {
        object[] objs = GetInputValues();
        T[] ts = new T[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] is T t) ts[i] = t;
        }
        return ts;
    }

    /// <summary> Return true if port is connected and has a valid input. </summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public bool TryGetInputValue<T>(out T value)
    {
        object obj = GetInputValue();
        if (obj is T t)
        {
            value = t;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary> Return the sum of all inputs. </summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public float GetInputSum(float fallback)
    {
        object[] objs = GetInputValues();
        if (objs.Length == 0) return fallback;
        float result = 0;
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] is float v) result += v;
        }
        return result;
    }

    /// <summary> Return the sum of all inputs. </summary>
    /// <returns> <see cref="NodePort.GetOutputValue"/> </returns>
    public int GetInputSum(int fallback)
    {
        object[] objs = GetInputValues();
        if (objs.Length == 0) return fallback;
        int result = 0;
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i] is int v) result += v;
        }
        return result;
    }

    /// <summary> Connect this <see cref="NodePort"/> to another </summary>
    /// <param name="port">The <see cref="NodePort"/> to connect to</param>
    public void Connect(NodePort port)
    {
        _connections ??= [];
        if (port == null) { Debug.LogWarning("Cannot connect to null port"); return; }
        if (port == this) { Debug.LogWarning("Cannot connect port to self."); return; }
        if (IsConnectedTo(port)) { Debug.LogWarning("Port already connected. "); return; }
        if (direction == port.direction) { Debug.LogWarning("Cannot connect two " + (direction == IO.Input ? "input" : "output") + " connections"); return; }

        //Undo.RecordObject(node, "Connect Port");
        //Undo.RecordObject(port.node, "Connect Port");

        if (port.connectionType == Node.ConnectionType.Override && port.ConnectionCount != 0) { port.ClearConnections(); }
        if (connectionType == Node.ConnectionType.Override && ConnectionCount != 0) { ClearConnections(); }
        _connections.Add(new PortConnection(port));
        port._connections ??= [];
        if (!port.IsConnectedTo(this)) port._connections.Add(new PortConnection(this));
        node.OnCreateConnection(this, port);
        port.node.OnCreateConnection(this, port);
    }

    public List<NodePort> GetConnections()
    {
        List<NodePort> result = [];
        for (int i = 0; i < _connections.Count; i++)
        {
            NodePort port = GetConnection(i);
            if (port != null) result.Add(port);
        }
        return result;
    }

    public NodePort GetConnection(int i)
    {
        //If the connection is broken for some reason, remove it.
        if (_connections[i].node == null || string.IsNullOrEmpty(_connections[i].fieldName))
        {
            _connections.RemoveAt(i);
            return null;
        }
        NodePort port = _connections[i].node.GetPort(_connections[i].fieldName);
        if (port == null)
        {
            _connections.RemoveAt(i);
            return null;
        }
        return port;
    }

    /// <summary> Get index of the connection connecting this and specified ports </summary>
    public int GetConnectionIndex(NodePort port)
    {
        for (int i = 0; i < ConnectionCount; i++)
        {
            if (_connections[i].Port == port) return i;
        }
        return -1;
    }

    /// <summary> Get instance id of the connection connecting this</summary>
    public int GetConnectionInstanceID(int i)
    {
        return _connections[i].InstanceID;
    }

    public bool IsConnectedTo(NodePort port)
    {
        for (int i = 0; i < _connections.Count; i++)
        {
            if (_connections[i].Port == port) return true;
        }
        return false;
    }

    /// <summary> Returns true if this port can connect to specified port </summary>
    public bool CanConnectTo(NodePort port)
    {
        // Determine which is input and which is output
        NodePort input = IsInput   ? this : port.IsInput  ? port : null;
        NodePort output = !IsInput ? this : !port.IsInput ? port : null;

        // Cannot connect to self
        if (input == output || input == null || output == null)
            return false;

        // Check input type constraints
        if (!CheckTypeConstraints(input, output))
            return false;

        // Check output type constraints
        if (!CheckTypeConstraints(output, input))
            return false;

        // Success
        return true;
    }

    private static bool CheckTypeConstraints(NodePort input, NodePort output)
    {
        switch (input.typeConstraint)
        {
            case Node.TypeConstraint.AssignableTo:
                if (!output.ValueType.IsAssignableTo(input.ValueType))
                    return false;
                break;
            case Node.TypeConstraint.Strict:
                if (input.ValueType != output.ValueType)
                    return false;
                break;
        }
        return true;
    }

    /// <summary> Disconnect this port from another port </summary>
    public void Disconnect(NodePort port)
    {
        // Remove this ports connection to the other
        for (int i = _connections.Count - 1; i >= 0; i--)
        {
            if (_connections[i].Port == port)
            {
                _connections.RemoveAt(i);
            }
        }
        if (port != null)
        {
            // Remove the other ports connection to this port
            for (int i = 0; i < port._connections.Count; i++)
            {
                if (port._connections[i].Port == this)
                {
                    port._connections.RemoveAt(i);
                    // Trigger OnRemoveConnection from this side port
                    port.node.OnRemoveConnection(port);
                }
            }
        }
        // Trigger OnRemoveConnection
        node.OnRemoveConnection(this);
    }

    /// <summary> Disconnect this port from another port </summary>
    public void Disconnect(int i)
    {
        // Remove the other ports connection to this port
        NodePort otherPort = _connections[i].Port;
        if (otherPort != null)
        {
            otherPort._connections.RemoveAll(it => { return it.Port == this; });
        }
        // Remove this ports connection to the other
        _connections.RemoveAt(i);

        // Trigger OnRemoveConnection
        node.OnRemoveConnection(this);
        if (otherPort != null) otherPort.node.OnRemoveConnection(otherPort);
    }

    public void ClearConnections()
    {
        while (_connections.Count > 0)
        {
            Disconnect(_connections[0].Port);
        }
    }

    /// <summary> Get reroute points for a given connection. This is used for organization </summary>
    public List<Vector2> GetReroutePoints(int index)
    {
        return _connections[index].reroutePoints;
    }

    /// <summary> Swap connections with another node </summary>
    public void SwapConnections(NodePort targetPort)
    {
        int aConnectionCount = _connections.Count;
        int bConnectionCount = targetPort._connections.Count;

        List<NodePort> portConnections = [];
        List<NodePort> targetPortConnections = [];

        // Cache port connections
        for (int i = 0; i < aConnectionCount; i++)
            portConnections.Add(_connections[i].Port);

        // Cache target port connections
        for (int i = 0; i < bConnectionCount; i++)
            targetPortConnections.Add(targetPort._connections[i].Port);

        ClearConnections();
        targetPort.ClearConnections();

        // Add port connections to targetPort
        for (int i = 0; i < portConnections.Count; i++)
            targetPort.Connect(portConnections[i]);

        // Add target port connections to this one
        for (int i = 0; i < targetPortConnections.Count; i++)
            Connect(targetPortConnections[i]);

    }

    /// <summary> Copy all connections pointing to a node and add them to this one </summary>
    public void AddConnections(NodePort targetPort)
    {
        int connectionCount = targetPort.ConnectionCount;
        for (int i = 0; i < connectionCount; i++)
        {
            PortConnection connection = targetPort._connections[i];
            NodePort otherPort = connection.Port;
            Connect(otherPort);
        }
    }

    /// <summary> Move all connections pointing to this node, to another node </summary>
    public void MoveConnections(NodePort targetPort)
    {
        int connectionCount = _connections.Count;

        // Add connections to target port
        for (int i = 0; i < connectionCount; i++)
        {
            PortConnection connection = targetPort._connections[i];
            NodePort otherPort = connection.Port;
            Connect(otherPort);
        }
        ClearConnections();
    }

    /// <summary> Swap connected nodes from the old list with nodes from the new list </summary>
    public void Redirect(List<Node> oldNodes, List<Node> newNodes)
    {
        foreach (PortConnection connection in _connections)
        {
            int index = oldNodes.IndexOf(connection.node);
            if (index >= 0) connection.node = newNodes[index];
        }
    }

    [Serializable]
    private class PortConnection
    {
        [SerializeField] public string fieldName;
        [SerializeField] public Node node;
        [SerializeField] public int InstanceID = 0;
        public NodePort Port { get { return _port ??= GetPort(); } }

        [SerializeIgnore] private NodePort _port;
        /// <summary> Extra connection path points for organization </summary>
        [SerializeField] public List<Vector2> reroutePoints = new List<Vector2>();

        public PortConnection() { } // for serialization

        public PortConnection(NodePort port)
        {
            _port = port;
            node = port.node;
            fieldName = port.fieldName;
            InstanceID = node.graph.NextID;
        }

        /// <summary> Returns the port that this <see cref="PortConnection"/> points to </summary>
        private NodePort GetPort()
        {
            if (node == null || string.IsNullOrEmpty(fieldName)) return null;
            return node.GetPort(fieldName);
        }
    }
}
