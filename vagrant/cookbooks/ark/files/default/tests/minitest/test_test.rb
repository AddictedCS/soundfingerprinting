require 'minitest/spec'

describe_recipe 'ark::test' do

  # It's often convenient to load these includes in a separate
  # helper along with
  # your own helper methods, but here we just include them directly:
  include MiniTest::Chef::Assertions
  include MiniTest::Chef::Context
  include MiniTest::Chef::Resources

  it "installed the unzip package" do
    package("unzip").must_be_installed
  end

  if RUBY_PLATFORM =~ /freebsd/
    it "installs the gnu tar package on freebsc" do
      package("gtar").must_be_installed
    end
  end

  it "puts an ark in the desired directory w/out symlinks" do
    directory("/usr/local/test_put").must_exist
  end

  it "dumps the correct files into place with correct owner and group" do
    file("/usr/local/foo_dump/foo1.txt").must_have(:owner, "foobarbaz").and(:group, "foobarbaz")
  end

  it "cherrypicks the mysql connector and set the correct owner and group" do
    file("/usr/local/foo_cherry_pick/foo1.txt").must_have(:owner, "foobarbaz").and(:group, "foobarbaz")
  end

  it "cherrypicks the file from a zip" do
    file("/usr/local/foo_cherry_pick_from_zip/foo1.txt").must_exist
  end

  it "creates directory and symlink properly for the full ark install" do
    directory("/usr/local/foo-2").must_have(:owner, "foobarbaz").and(:group, "foobarbaz")
    link("/usr/local/foo").must_exist.with(:link_type, :symbolic).and(:to, "/usr/local/foo-2")
  end

  it "symlinks multiple binary commands" do
    link("/usr/local/bin/do_foo").must_exist.with(:link_type, :symbolic).and(:to, "/usr/local/foo-2/bin/do_foo")
  link("/usr/local/bin/do_more_foo").must_exist.with(:link_type, :symbolic).and(:to, "/usr/local/foo-2/bin/do_more_foo")
  end

  it "appends to the environment PATH" do
    unless RUBY_PLATFORM =~ /freebsd/
      file("/etc/profile.d/foo_append_env.sh").must_include '/usr/local/foo_append_env-7.0.26/bin'

      bin_path_present = !ENV['PATH'].scan( '/usr/local/foo_append_env-7.0.26/bin').empty?
      assert bin_path_present
    end
  end

  it "doesn't strip top-level directory if specified" do
    directory( "/usr/local/foo_dont_strip/foo_sub").must_exist
  end

  it "does strip for zip file" do
    file("/usr/local/foo_zip_strip/foo1.txt").must_exist
  end


  it "successfully compiles haproxy" do
    file("/usr/local/haproxy-1.5/haproxy").must_exist
  end

  unless RUBY_PLATFORM =~ /freebsd/
    it "installs haproxy binary" do
      file("/usr/local/sbin/haproxy").must_exist
      directory("/usr/local/doc/haproxy").must_exist
    end
  end

  it "creates an alternate prefix_bin" do
    link("/opt/bin/do_foo").must_exist.with(:link_type, :symbolic).and(:to, "/opt/foo_alt_bin-3/bin/do_foo")
  end

  it "properly unpacks .tbz and .tgz archives" do
    file("/usr/local/foo_tbz/foo1.txt").must_exist
    file("/usr/local/foo_tgz/foo1.txt").must_exist
  end

  it "sends notification when resource updated" do
    file("/tmp/foobarbaz/notification_successful.txt").must_exist
  end

  it "uses autogen.sh to generate configure script" do
    file("/usr/local/test_autogen-1/configure").must_exist
  end

end
