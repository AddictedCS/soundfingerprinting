ntp Cookbook CHANGELOG
======================
This file is used to list changes made in each version of the ntp cookbook.


v1.5.0
------
### Improvement
- **[COOK-3651](https://tickets.opscode.com/browse/COOK-3651)** - Refactor and clean up
- **[COOK-3630](https://tickets.opscode.com/browse/COOK-3630)** - Switch NTP cookbook linting from Tailor to Rubocop
- **[COOK-3273](https://tickets.opscode.com/browse/COOK-3273)** - Add tests

### New Feature
- **[COOK-3636](https://tickets.opscode.com/browse/COOK-3636)** - Allow ntp cookbook to update clock to ntp servers

### Bug
- **[COOK-3410](https://tickets.opscode.com/browse/COOK-3410)** - Remove redundant ntpdate/disable recipes
- **[COOK-1170](https://tickets.opscode.com/browse/COOK-1170)** - Allow redefining NTP servers in a role


v1.4.0
------
### Improvement
- **[COOK-3365](https://tickets.opscode.com/browse/COOK-3365)** - Update ntp leapseconds file to version 3597177600
- **[COOK-1674](https://tickets.opscode.com/browse/COOK-1674)** - Add Windows support

v1.3.2
------
- [COOK-2024] - update leapfile for IERS Bulletin C

v1.3.0
------
- [COOK-1404] - add leapfile for handling leap seconds

v1.2.0
------
- [COOK-1184] - Add recipe to disable NTP completely
- [COOK-1298] - Refactor into a reference cookbook for testing

v1.1.8
------
- [COOK-1158] - RHEL family >= 6 has ntpdate package

v1.1.6
------
- Related to changes in COOK-1124, fix group for freebsd and else

v1.1.4
------
- [COOK-1124] - parameterised driftfile and statsdir to be configurable by platform

v1.1.2
------
- [COOK-952] - freebsd support
- [COOK-949] - check for any virtual system not just vmware

v1.1.0
------
- Fixes COOK-376 (use LAN peers, iburst option, LAN restriction attribute)

v1.0.1
------
- Support scientific linux
- Use service name attribute in resource (fixes EL derivatives)
