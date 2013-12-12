# require 'fileutils'

# remove file so we can test sending notification on its creation
if ::File.exist? "/tmp/foobarbaz/foo1.txt"
  FileUtils.rm_f "/tmp/foobarbaz/foo1.txt"
end

ruby_block "test_notification" do
  block do
    if ::File.exist? "/tmp/foobarbaz/foo1.txt"
      FileUtils.touch "/tmp/foobarbaz/notification_successful.txt"
    end
  end
  action :nothing
end

user 'foobarbaz'

directory "/opt/bin" do
  recursive true
end

ark "foo" do
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tar.gz'
  checksum '5996e676f17457c823d86f1605eaa44ca8a81e70d6a0e5f8e45b51e62e0c52e8'
  version '2'
  prefix_root "/usr/local"
  owner "foobarbaz"
  group 'foobarbaz'
  has_binaries [ 'bin/do_foo', 'bin/do_more_foo' ]
  action :install
end

ark 'test_put' do
  url  'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tar.gz'
  checksum '5996e676f17457c823d86f1605eaa44ca8a81e70d6a0e5f8e45b51e62e0c52e8'
  owner 'foobarbaz'
  group 'foobarbaz'
  action :put
end

ark "test_dump" do
  url  'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.zip'
  checksum 'deea3a324115c9ca0f3078362f807250080bf1b27516f7eca9d34aad863a11e0'
  path '/usr/local/foo_dump'
  creates 'foo1.txt'
  owner 'foobarbaz'
  group 'foobarbaz'
  action :dump
end

ark 'cherry_pick_test' do
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tar.gz'
  checksum '5996e676f17457c823d86f1605eaa44ca8a81e70d6a0e5f8e45b51e62e0c52e8'
  path '/usr/local/foo_cherry_pick'
  owner 'foobarbaz'
  group 'foobarbaz'
  creates "foo_sub/foo1.txt"
  action :cherry_pick
end

ark 'cherry_pick_with_zip' do
  url  'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.zip'
  checksum 'deea3a324115c9ca0f3078362f807250080bf1b27516f7eca9d34aad863a11e0'
  path '/usr/local/foo_cherry_pick_from_zip'
  creates "foo_sub/foo1.txt"
  action :cherry_pick
end

ark "foo_append_env" do
  version "7.0.26"
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tar.gz'
  checksum '5996e676f17457c823d86f1605eaa44ca8a81e70d6a0e5f8e45b51e62e0c52e8'
  append_env_path true
  action :install
end

ark "foo_dont_strip" do
  version "2"
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tar.gz'
  checksum '5996e676f17457c823d86f1605eaa44ca8a81e70d6a0e5f8e45b51e62e0c52e8'
  strip_leading_dir false
  action :install
end

ark "foo_zip_strip" do
  version "2"
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.zip'
  checksum 'deea3a324115c9ca0f3078362f807250080bf1b27516f7eca9d34aad863a11e0'
  action :install
end

ark "haproxy" do
  url  "http://haproxy.1wt.eu/download/1.5/src/snapshot/haproxy-ss-20120403.tar.gz"
  version "1.5"
  checksum 'ba0424bf7d23b3a607ee24bbb855bb0ea347d7ffde0bec0cb12a89623cbaf911'
  make_opts [ 'TARGET=linux26' ]
  action :install_with_make
end unless platform?("freebsd")

ark "foo_alt_bin" do
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tar.gz'
  checksum '5996e676f17457c823d86f1605eaa44ca8a81e70d6a0e5f8e45b51e62e0c52e8'
  version '3'
  prefix_root "/opt"
  prefix_home "/opt"
  prefix_bin "/opt/bin"
  owner "foobarbaz"
  group 'foobarbaz'
  has_binaries [ 'bin/do_foo' ]
  action :install
end

ark "foo_tbz" do
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tbz'
  version '3'
end

ark "foo_tgz" do
  url 'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.tgz'
  version '3'
end

ark "test notification" do
  url  'https://github.com/bryanwb/chef-ark/raw/master/files/default/foo.zip'
  path "/tmp/foobarbaz"
  creates "foo1.txt"
  action :dump
  notifies :create, "ruby_block[test_notification]", :immediately
end

ark "test_autogen" do
  url 'https://github.com/zeromq/libzmq/tarball/master'
  extension "tar.gz"
  action :configure
  # autoconf in RHEL < 6 is too old
  not_if { platform_family?('rhel') && node['platform_version'].to_f < 6.0 }
end
