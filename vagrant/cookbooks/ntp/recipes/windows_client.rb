#
# Cookbook Name:: ntp
# Recipe:: windows_client
# Author:: Tim Smith (<tsmith@limelight.com>)
#
# Copyright 2012, Webtrends, Inc
# Copyright 2013, Limelight Networks, Inc
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

# Do not continue if trying to run the Windows recipe on non-Windows platform
return 'The ntp::windows_client recipe only supports Windows' unless platform_family?('windows')

directory 'C:/NTP/etc' do
  inherits  true
  action    :create
  recursive true
end

cookbook_file 'C:/NTP/ntp.ini' do
  source   'ntp.ini'
  inherits true
  action   :create
end

windows_package node['ntp']['vs_runtime_productname'] do
  source         node['ntp']['vs_runtime_url']
  options        '/q'
  installer_type :custom
  action         :install
  only_if        { node['kernel']['release'].to_f < 6 }
end

unless File.exists?('C:/NTP/bin/ntpd.exe')
  remote_file "#{Chef::Config[:file_cache_path]}/ntpd.exe" do
    source node['ntp']['package_url']
  end

  execute 'ntpd_install' do
    command "#{Chef::Config[:file_cache_path]}\\ntpd.exe /USEFILE=C:\\NTP\\ntp.ini"
    returns [0, 2]
  end
end
