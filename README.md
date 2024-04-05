<p align="center">
  <img src="logo.png" width="350" title="hover text">
</p>

# Throttlr

Throttlr is a C# library designed to facilitate highly configurable, generic request throttling.
It supports basic rate limiting as well as bandwidth limiting using multiple window types.
Its multi-layer caching strategy leverages Redis as its primary storage mechanism for tracking request counts and timings.
In the event of a Redis failure or unavailability, Throttlr seamlessly switches to an in-memory cache as a failover mechanism, ensuring continuous service and consistent rate limiting.
This provides developers with a robust and resilient throttling solution, ideal for applications needing to maintain stability and performance under varying traffic loads.

# ...
