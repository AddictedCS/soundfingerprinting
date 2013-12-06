#
# Author:: Seth Chisamore (<schisamo@opscode.com>)
# Cookbook Name:: java
# Attributes:: default
#
# Copyright 2010, Opscode, Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

# remove the deprecated Ubuntu jdk packages
default['java']['remove_deprecated_packages'] = false

# default jdk attributes
default['java']['install_flavor'] = "openjdk"
default['java']['jdk_version'] = '6'
default['java']['arch'] = kernel['machine'] =~ /x86_64/ ? "x86_64" : "i586"
default['java']['openjdk_packages'] = []
default['java']['accept_license_agreement'] = false

case node['platform_family']
when "rhel", "fedora"
  default['java']['java_home'] = "/usr/lib/jvm/java"
  default['java']['openjdk_packages'] = ["java-1.#{node['java']['jdk_version']}.0-openjdk", "java-1.#{node['java']['jdk_version']}.0-openjdk-devel"]
when "freebsd"
  default['java']['java_home'] = "/usr/local/openjdk#{node['java']['jdk_version']}"
  default['java']['openjdk_packages'] = ["openjdk#{node['java']['jdk_version']}"]
when "arch"
  default['java']['java_home'] = "/usr/lib/jvm/java-#{node['java']['jdk_version']}-openjdk"
  default['java']['openjdk_packages'] = ["openjdk#{node['java']['jdk_version']}}"]
when "windows"
  default['java']['install_flavor'] = "windows"
  default['java']['windows']['url'] = nil
  default['java']['windows']['checksum'] = nil
  default['java']['windows']['package_name'] = "Java(TM) SE Development Kit 7 (64-bit)"
when "debian"
  default['java']['java_home'] = "/usr/lib/jvm/java-#{node['java']['jdk_version']}-#{node['java']['install_flavor']}"
  # Newer Debian & Ubuntu adds the architecture to the path
  if node['platform'] == 'debian' && Chef::VersionConstraint.new(">= 7.0").include?(node['platform_version']) ||
     node['platform'] == 'ubuntu' && Chef::VersionConstraint.new(">= 12.04").include?(node['platform_version'])
    default['java']['java_home'] = "#{node['java']['java_home']}-#{node['kernel']['machine'] == 'x86_64' ? 'amd64' : 'i386'}"
  end
  default['java']['openjdk_packages'] = ["openjdk-#{node['java']['jdk_version']}-jdk", "openjdk-#{node['java']['jdk_version']}-jre-headless"]
when "smartos"
  default['java']['java_home'] = "/opt/local/java/sun6"
  default['java']['openjdk_packages'] = ["sun-jdk#{node['java']['jdk_version']}", "sun-jre#{node['java']['jdk_version']}"]
else
  default['java']['java_home'] = "/usr/lib/jvm/default-java"
  default['java']['openjdk_packages'] = ["openjdk-#{node['java']['jdk_version']}-jdk"]
end

case node['java']['install_flavor']
when 'ibm'
  default['java']['ibm']['url'] = nil
  default['java']['ibm']['checksum'] = nil
  default['java']['ibm']['accept_ibm_download_terms'] = false
  default['java']['java_home'] = "/opt/ibm/java"
when 'oracle_rpm'
  default['java']['oracle_rpm']['type'] = 'jdk'
  default['java']['java_home'] = "/usr/java/latest"
end

# if you change this to true, you can download directly from Oracle
default['java']['oracle']['accept_oracle_download_terms'] = false

# direct download paths for oracle, you have been warned!

# jdk6 attributes
default['java']['jdk']['6']['bin_cmds'] = [ "appletviewer", "apt", "ControlPanel", "extcheck", "HtmlConverter", "idlj", "jar", "jarsigner",
                                            "java", "javac", "javadoc", "javah", "javap", "javaws", "jconsole", "jcontrol", "jdb", "jhat",
                                            "jinfo", "jmap", "jps", "jrunscript", "jsadebugd", "jstack", "jstat", "jstatd", "jvisualvm",
                                            "keytool", "native2ascii", "orbd", "pack200", "policytool", "rmic", "rmid", "rmiregistry",
                                            "schemagen", "serialver", "servertool", "tnameserv", "unpack200", "wsgen", "wsimport", "xjc" ]

# x86_64
default['java']['jdk']['6']['x86_64']['url'] = 'http://download.oracle.com/otn-pub/java/jdk/6u45-b06/jdk-6u45-linux-x64.bin'
default['java']['jdk']['6']['x86_64']['checksum'] = '6b493aeab16c940cae9e3d07ad2a5c5684fb49cf06c5d44c400c7993db0d12e8'

# i586
default['java']['jdk']['6']['i586']['url'] = 'http://download.oracle.com/otn-pub/java/jdk/6u45-b06/jdk-6u45-linux-i586.bin'
default['java']['jdk']['6']['i586']['checksum'] = 'd53b5a2518d80e1d95565f0adda54eee229dc5f4a1d1a3c2f7bf5045b168a357'

# jdk7 attributes

default['java']['jdk']['7']['bin_cmds'] = [ "appletviewer", "apt", "ControlPanel", "extcheck", "idlj", "jar", "jarsigner", "java", "javac",
                                            "javadoc", "javafxpackager", "javah", "javap", "javaws", "jcmd", "jconsole", "jcontrol", "jdb",
                                            "jhat", "jinfo", "jmap", "jps", "jrunscript", "jsadebugd", "jstack", "jstat", "jstatd", "jvisualvm",
                                            "keytool", "native2ascii", "orbd", "pack200", "policytool", "rmic", "rmid", "rmiregistry",
                                            "schemagen", "serialver", "servertool", "tnameserv", "unpack200", "wsgen", "wsimport", "xjc" ]

# x86_64
default['java']['jdk']['7']['x86_64']['url'] = 'http://download.oracle.com/otn-pub/java/jdk/7u25-b15/jdk-7u25-linux-x64.tar.gz'
default['java']['jdk']['7']['x86_64']['checksum'] = 'f80dff0e19ca8d038cf7fe3aaa89538496b80950f4d10ff5f457988ae159b2a6'

# i586
default['java']['jdk']['7']['i586']['url'] = 'http://download.oracle.com/otn-pub/java/jdk/7u25-b15/jdk-7u25-linux-i586.tar.gz'
default['java']['jdk']['7']['i586']['checksum'] = 'dd89b20afa939992bb7fdc44837fa64f0a98d7ee1e5706fe8a2d9e2247ba6de7'
