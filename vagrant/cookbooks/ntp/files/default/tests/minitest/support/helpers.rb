module Helpers
  # Helper modules for NTP cookbook minitest
  module Ntp
    include MiniTest::Chef::Assertions
    include MiniTest::Chef::Context
    include MiniTest::Chef::Resources
  end
end
