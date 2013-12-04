# libs

module Opscode
  module Ark
    module ProviderHelpers
      private

      def unpack_type
        case parse_file_extension
        when /tar.gz|tgz/  then "tar_xzf"
        when /tar.bz2|tbz/ then "tar_xjf"
        when /zip|war|jar/ then "unzip"
        else raise "Don't know how to expand #{new_resource.url}"
        end
      end

      def parse_file_extension
        if new_resource.extension.nil?
          # purge any trailing redirect
          url = new_resource.url.clone
          url =~ /^https?:\/\/.*(.gz|bz2|bin|zip|jar|tgz|tbz)(\/.*\/)/
          url.gsub!($2, '') unless $2.nil?
          # remove tailing query string
          release_basename = ::File.basename(url.gsub(/\?.*\z/, '')).gsub(/-bin\b/, '')
          # (\?.*)? accounts for a trailing querystring
          Chef::Log.debug("DEBUG: release_basename is #{release_basename}")
          release_basename =~ %r{^(.+?)\.(tar\.gz|tar\.bz2|zip|war|jar|tgz|tbz)(\?.*)?}
          Chef::Log.debug("DEBUG: file_extension is #{$2}")
          new_resource.extension = $2
        end
        new_resource.extension
      end

      def unpack_command
        case unpack_type
        when "tar_xzf"
          cmd = node['ark']['tar']
          cmd = cmd + " xzf "
          cmd = cmd + new_resource.release_file
          cmd = cmd + tar_strip_args
        when "tar_xjf"
          cmd = node['ark']['tar']
          cmd = cmd + " xjf "
          cmd = cmd + " #{new_resource.release_file}"
          cmd = cmd + tar_strip_args
        when "unzip"
          cmd = unzip_command
        end
        Chef::Log.debug("DEBUG: cmd: #{cmd}")
        cmd
      end

      def unzip_command
        if new_resource.strip_leading_dir
          require 'tmpdir'
          tmpdir = Dir.mktmpdir
          cmd = "unzip -q -u -o #{new_resource.release_file} -d #{tmpdir}"
          cmd = cmd + "&& rsync -a #{tmpdir}/*/ #{new_resource.path}"
          cmd = cmd + "&& rm -rf  #{tmpdir}"
        else
          cmd = "unzip -q -u -o #{new_resource.release_file} -d #{new_resource.path}"
        end
      end

      def dump_command
        case unpack_type
        when "tar_xzf", "tar_xjf"
          cmd = "tar -mxf \"#{new_resource.release_file}\" -C \"#{new_resource.path}\""
        when "unzip"
          cmd = "unzip  -j -q -u -o \"#{new_resource.release_file}\" -d \"#{new_resource.path}\""
        end
        Chef::Log.debug("DEBUG: cmd: #{cmd}")
        cmd
      end

      def cherry_pick_command
        cmd = node['ark']['tar']

        case unpack_type
        when "tar_xzf"
          cmd = cmd + " xzf "
          cmd = cmd + " #{new_resource.release_file}"
          cmd = cmd + " -C"
          cmd = cmd + " #{new_resource.path}"
          cmd = cmd + " #{new_resource.creates}"
          cmd = cmd + tar_strip_args
        when "tar_xjf"
          cmd = cmd + "xjf #{new_resource.release_file}"
          cmd = cmd + "-C #{new_resource.path} #{new_resource.creates}"
          cmd = cmd + tar_strip_args
        when "unzip"
          cmd = "unzip -t #{new_resource.release_file} \"*/#{new_resource.creates}\" ; stat=$? ;"
          cmd = cmd + "if [ $stat -eq 11 ] ; then "
          cmd = cmd + "unzip  -j -o #{new_resource.release_file} \"#{new_resource.creates}\" -d #{new_resource.path} ;"
          cmd = cmd + "elif [ $stat -ne 0 ] ; then false ;"
          cmd = cmd + "else "
          cmd = cmd + "unzip  -j -o #{new_resource.release_file} \"*/#{new_resource.creates}\" -d #{new_resource.path} ;"
          cmd = cmd + "fi"
        end
        Chef::Log.debug("DEBUG: cmd: #{cmd}")
        cmd
      end

      def set_paths
        release_ext = parse_file_extension
        prefix_bin  = new_resource.prefix_bin.nil? ? new_resource.run_context.node['ark']['prefix_bin'] : new_resource.prefix_bin
        prefix_root = new_resource.prefix_root.nil? ? new_resource.run_context.node['ark']['prefix_root'] : new_resource.prefix_root
        if new_resource.prefix_home.nil?
          default_home_dir = ::File.join(new_resource.run_context.node['ark']['prefix_home'], new_resource.name)
        else
          default_home_dir =  ::File.join(new_resource.prefix_home, new_resource.name)
        end
        # set effective paths
        new_resource.prefix_bin = prefix_bin
        new_resource.version ||= "1"  # initialize to one if nil
        new_resource.path       = ::File.join(prefix_root, "#{new_resource.name}-#{new_resource.version}")
        new_resource.home_dir ||= default_home_dir
        Chef::Log.debug("path is #{new_resource.path}")
        new_resource.release_file     = ::File.join(Chef::Config[:file_cache_path],  "#{new_resource.name}.#{release_ext}")
      end

      def set_put_paths
        release_ext = parse_file_extension
        path = new_resource.path.nil? ? new_resource.run_context.node['ark']['prefix_root'] : new_resource.path
        new_resource.path      = ::File.join(path, new_resource.name)
        Chef::Log.debug("DEBUG: path is #{new_resource.path}")
        new_resource.release_file     = ::File.join(Chef::Config[:file_cache_path],  "#{new_resource.name}.#{release_ext}")
      end

      def set_dump_paths
        release_ext = parse_file_extension
        new_resource.release_file  = ::File.join(Chef::Config[:file_cache_path],  "#{new_resource.name}.#{release_ext}")
      end

      def set_apache_url(url_ref)
        raise "Missing required resource attribute url" unless url_ref
        url_ref.gsub!(/:name:/,          name.to_s)
        url_ref.gsub!(/:version:/,       version.to_s)
        url_ref.gsub!(/:apache_mirror:/, node['install_from']['apache_mirror'])
        url_ref
      end

      def tar_strip_args
        new_resource.strip_leading_dir ? " --strip-components=1" : ""
      end

      # def unpacked?(path)
      #   if new_resource.creates
      #     full_path = ::File.join(new_resource.path, new_resource.creates)
      #   else
      #     full_path = path
      #   end
      #   if ::File.directory? full_path
      #     if ::File.stat(full_path).nlink == 2
      #       false
      #     else
      #       true
      #     end
      #   elsif ::File.exists? full_path
      #     true
      #   else
      #     false
      #   end
      # end
    end
  end
end
