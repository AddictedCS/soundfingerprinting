actions :auto_attach

default_action :auto_attach

attribute :aws_access_key,        :kind_of => String
attribute :aws_secret_access_key, :kind_of => String
attribute :mount_point,           :kind_of => String
attribute :mount_point_owner,     :kind_of => String, :default => 'root'
attribute :mount_point_group,     :kind_of => String, :default => 'root'
attribute :mount_point_mode,      :kind_of => String, :default => 00755
attribute :disk_count,            :kind_of => Integer
attribute :disk_size,             :kind_of => Integer
attribute :level,                 :default => 10
attribute :filesystem,            :default => "ext4"
attribute :filesystem_options,    :default => "rw,noatime,nobootwait"
attribute :snapshots,             :default => []
attribute :disk_type,             :kind_of => String, :default => 'standard'
attribute :disk_piops,            :kind_of => Integer, :default => 0

