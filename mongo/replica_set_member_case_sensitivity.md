# Replica Set Member Case Sensitivity

Recently we ran into an issue with Mongo which caused it to lose access to 3 of the 5 replica set members, halting write access to the entire system.  After looking into the issue, we determined that there were duplicate members in the replica set.  

Mongo accepted duplicate members because the DNS names were cased differently.  A recent change to our backend VM management system resulted in lower case machine names being returned for a subset of machines.  When the service configuration scripts ran, the *new machines* were added to the replica set, and unfortunatly due to a bug in the script, the non-matching machines were not removed.  Only when attempting to resolve the replica set configuration did it determine that these were the same member and instead of ignoring one member, the system indicated that it was unable to access either duplicate.

We updated our configuration scripts in order to do case insensitive match (and remove non-existant machines) and the system continued on it's merry way.
