require File.expand_path('../support/helpers', __FILE__)

describe 'ntp::undo' do
  include Helpers::Ntp

  it 'disables the NTP daemon' do
    service(node['ntp']['service']).wont_be_running
    service(node['ntp']['service']).wont_be_enabled
  end

  it 'removes the NTP packages' do
    node['ntp']['packages'].each do |p|
      package(p).wont_be_installed
    end
  end
end
