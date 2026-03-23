namespace NexusProxy.Core.Interfaces;

public interface ILoadBalancerFactory
{
    ILoadBalancer GetLoadBalancer();
}
