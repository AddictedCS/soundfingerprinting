Java Cookbook CHANGELOG
=======================
This file is used to list changes made in each version of the Java cookbook.


v1.15.4
-------
[COOK-4210] - remove unneeded run_command to prevent zombie processes


v1.15.2
-------
[CHEF-4210] remove unneeded run_command to prevent zombie processes


v1.15.0
-------
### Bug
- Fixing version number. Accidently released at 0.15.x instead of 1.15.x


v0.15.2
-------
### FIX
- [COOK-3908] - Fixing JAVA_HOME on Ubuntu 10.04


v1.14.0
-------
### Bug
- **[COOK-3704](https://tickets.opscode.com/browse/COOK-3704)** - Fix alternatives when the package is already installed
- **[COOK-3668](https://tickets.opscode.com/browse/COOK-3668)** - Fix a condition that would result in an error executing action `run` on resource 'bash[update-java-alternatives]'
- **[COOK-3569](https://tickets.opscode.com/browse/COOK-3569)** - Fix bad checksum length
- **[COOK-3541](https://tickets.opscode.com/browse/COOK-3541)** - Fix an issue where Java cookbook installs both JDK 6 and JDK 7 when JDK 7 is specified
- **[COOK-3518](https://tickets.opscode.com/browse/COOK-3518)** - Allow Windoes recipe to download from signed S3 url
- **[COOK-2996](https://tickets.opscode.com/browse/COOK-2996)** - Fix a failure on Centos 6.4 and Oracle JDK 7

### Improvement
- **[COOK-2793](https://tickets.opscode.com/browse/COOK-2793)** - Improve Windows support


v1.13.0
-------
### Bug
- **[COOK-3295](https://tickets.opscode.com/browse/COOK-3295)** - Add default `platform_family` option in Java helper
- **[COOK-3277](https://tickets.opscode.com/browse/COOK-3277)** - Fix support for Fedora

### Improvement
- **[COOK-3278](https://tickets.opscode.com/browse/COOK-3278)** - Upgrade to Oracle Java 7u25
- **[COOK-3029](https://tickets.opscode.com/browse/COOK-3029)** - Add Oracle RPM support
- **[COOK-2931](https://tickets.opscode.com/browse/COOK-2931)** - Add support for the platform `xenserver`
- **[COOK-2154](https://tickets.opscode.com/browse/COOK-2154)** - Add SmartOS support

v1.12.0
-------
### Improvement
- [COOK-2154]: Add SmartOS support to java::openjdk recipe
- [COOK-3278]: upgrade to Oracle Java 7u25

### Bug
- [COOK-2931]: Adding support for the platform 'xenserver' (for installations of java in DOM0)
- [COOK-3277]: java cookbook fails on Fedora

v1.11.6
-------
### Bug
- [COOK-2847]: Java cookbook does not have opensuse support
- [COOK-3142]: Syntax Errors spec/default_spec.rb:4-8

v1.11.4
-------
### Bug
- [COOK-2989]: `bash[update-java-alternatives]` resource uses wrong attribute

v1.11.2
-------
### Bug
- Use SHA256 checksums for Oracle downloads, not SHA1.

v1.11.0
-------
This version brings a wealth of tests and (backwards-compatible) refactoring, plus some new features (updated Java, IBM recipe).

### Sub-task
- [COOK-2897]: Add ibm recipe to java cookbook
- [COOK-2903]: move java_home resources to their own recipe
- [COOK-2904]: refactor ruby_block "update-java-alternatives"
- [COOK-2905]: use platform_family in java cookbook
- [COOK-2920]: add chefspec to java cookbook

### Task
- [COOK-2902]: Refactor java cookbook

### Improvement
- [COOK-2900]: update JDK to JDK 7u21, 6u45

v1.10.2
-------
- [COOK-2415] - Fixed deprecation warnings in ark provider and openjdk recipe by using Chef::Mixin::ShellOut instead of Chef::ShellOut

v1.10.0
-------
- [COOK-2400] - Allow java ark :url to be https
- [COOK-2436] - Upgrade needed for oracle jdk in java cookbook

v1.9.6
------
- [COOK-2412] - add support for Oracle Linux

v1.9.4
------
- [COOK-2083] - Run set-env-java-home in Java cookbook only if necessary
- [COOK-2332] - ark provider does not allow for *.tgz tarballs to be used
- [COOK-2345] - Java cookbook fails on CentOS6 (update-java-alternatives)

v1.9.2
------
- [COOK-2306] - FoodCritic fixes for java cookbook

v1.9.0
------
- [COOK-2236] - Update the Oracle Java version in the Java cookbook to release 1.7u11

v1.8.2
------
- [COOK-2205] - Fix for missing /usr/lib/jvm/default-java on Debian

v1.8.0
------
- [COOK-2095] - Add windows support

v1.7.0
------
- [COOK-2001] - improvements for Oracle update-alternatives
  - When installing an Oracle JDK it is now registered with a higher
    priority than OpenJDK. (Related to COOK-1131.)
  - When running both the oracle and oracle_i386 recipes, alternatives
    are now created for both JDKs.
  - Alternatives are now created for all binaries listed in version
    specific attributes. (Related to COOK-1563 and COOK-1635.)
  - When installing Oracke JDKs on Ubuntu, create .jinfo files for use
    with update-java-alternatives. Commands to set/install
    alternatives now only run if needed.

v1.6.4
------
- [COOK-1930] - fixed typo in attribute for java 5 on i586

v1.6.2
------
- whyrun support in `java_ark` LWRP
- CHEF-1804 compatibility
- [COOK-1786]- install Java 6u37 and Java 7u9
- [COOK-1819] -incorrect warning text about `node['java']['oracle']['accept_oracle_download_terms']`

v1.6.0
------
- [COOK-1218] - Install Oracle JDK from Oracle download directly
- [COOK-1631] - set JAVA_HOME in openjdk recipe
- [COOK-1655] - Install correct architecture on Amazon Linux

v1.5.4
------
- [COOK-885] - update alternatives called on wrong file
- [COOK-1607] - use shellout instead of execute resource to update alternatives

v1.5.2
------
- [COOK-1200] - remove sun-java6-jre on Ubuntu before installing Oracle's Java
- [COOK-1260] - fails on Ubuntu 12.04 64bit with openjdk7
- [COOK-1265] - Oracle Java should symlink the jar command

v1.5.0
------
- [COOK-1146] - Oracle now prevents download of JDK via non-browser
- [COOK-1114] - fix File.exists?

v1.4.2
------
- [COOK-1051] - fix attributes typo and platform case switch consistency

v1.4.0
------
- [COOK-858] - numerous updates: handle jdk6 and 7, switch from sun to oracle, make openjdk default, add `java_ark` LWRP.
- [COOK-942] - FreeBSD support
- [COOK-520] - ArchLinux support
