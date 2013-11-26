# Author:: Bryan W. Berry (<bryan.berry@gmail.com>)
# Author:: Seth Chisamore (<schisamo@opscode.com>)
# Author:: Joshua Timberman (<joshua@opscode.com>)
#
# Cookbook Name:: java
# Recipe:: openjdk
#
# Copyright 2010-2013, Opscode, Inc.
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

java_location = Opscode::OpenJDK.new(node).java_location

if platform_requires_license_acceptance?
  file "/opt/local/.dlj_license_accepted" do
    owner "root"
    group "root"
    mode "0400"
    action :create
    only_if { node['java']['accept_license_agreement'] }
  end
end

node['java']['openjdk_packages'].each do |pkg|
  package pkg
end

if platform_family?('debian', 'rhel', 'fedora')
  bash 'update-java-alternatives' do
    code <<-EOH.gsub(/^\s+/, '')
      update-alternatives --install /usr/bin/java java #{java_location} 1061 && \
      update-alternatives --set java #{java_location}
    EOH
    only_if "update-alternatives --display java | grep '#{java_location} - priority 1061'"
  end
end

# We must include this recipe AFTER updating the alternatives or else JAVA_HOME
# will not point to the correct java.
include_recipe 'java::set_java_home'
