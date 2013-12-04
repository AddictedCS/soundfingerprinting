require File.expand_path('../support/helpers', __FILE__)

describe 'ntp::default' do
  include Helpers::Ntp

  it 'starts the NTP daemon' do
    service(node['ntp']['service']).must_be_running
    service(node['ntp']['service']).must_be_enabled
  end

  it 'creates the leapfile' do
    file(node['ntp']['leapfile']).must_exist.with(:owner, node['ntp']['conf_owner']).and(:group, node['ntp']['conf_group'])
  end

  it 'creates the ntp.conf' do
    file('/etc/ntp.conf').must_exist.with(:owner, node['ntp']['conf_owner']).and(:group, node['ntp']['conf_group'])

    node['ntp']['servers'].each do |s|
      file('/etc/ntp.conf').must_include s
    end
  end
end
