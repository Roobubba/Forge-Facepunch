﻿using BeardedManStudios.Forge.Networking.Frame;

namespace BeardedManStudios.Forge.Networking
{
	/// <summary>
	/// Every program that uses NetworkObject will need to have a factory
	/// to create objects with, this is the interface that it must implement
	/// </summary>
	public interface INetworkObjectFactory
	{
		/// <summary>
		/// There has been a request on the network to create a type of NetworkObject and this
		/// is the entry point for that message
		/// </summary>
		/// <param name="networker">The NetWorker that is the controller for the soon to be created NetworkObject</param>
		/// <param name="identity">The identity for the NetworkObject as an int so that this factory knows what kind of NetworkObject to create</param>
		/// <param name="id">The id that the server has assigned this new NetworkObject (if client)</param>
		/// <param name="frame">The data that was received on the network about this creation</param>
		/// <param name="callback">The callback with the network object that was successfully created</param>
		/// <returns></returns>
		void NetworkCreateObject(NetWorker networker, int identity, uint id, FrameStream frame, System.Action<NetworkObject> callback);
	}
}
