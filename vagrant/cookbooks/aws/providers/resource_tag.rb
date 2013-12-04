include Opscode::Aws::Ec2

action :add do

  unless @new_resource.resource_id
    resource_id = @new_resource.name
  else
    resource_id = @new_resource.resource_id
  end

  @new_resource.tags.each do |k,v|
    unless @current_resource.tags.keys.include?(k)
      converge_by("add tag '#{k}' with value '#{v}' on resource #{resource_id}") do
        ec2.create_tags(resource_id, { k => v })
        Chef::Log.info("AWS: Added tag '#{k}' with value '#{v}' on resource #{resource_id}")
      end
    else
      Chef::Log.debug("AWS: Resource #{resource_id} already has a tag with key '#{k}', will not add tag '#{k}' => '#{v}'")
    end
  end
end

action :update do
  unless @new_resource.resource_id
    resource_id = @new_resource.name
  else
    resource_id = @new_resource.resource_id
  end

  updated_tags = @current_resource.tags.merge(@new_resource.tags)
  unless updated_tags.eql?(@current_resource.tags)
    # tags that begin with "aws" are reserved
    converge_by("Updating the following tags for resource #{resource_id} (skipping AWS tags): " + updated_tags.inspect) do
      Chef::Log.info("AWS: Updating the following tags for resource #{resource_id} (skipping AWS tags): " + updated_tags.inspect)
      updated_tags.delete_if { |key, value| key.to_s.match /^aws/ }
      ec2.create_tags(resource_id, updated_tags)
    end
  else
    Chef::Log.debug("AWS: Tags for resource #{resource_id} are unchanged")
  end
end

action :remove do
  unless @new_resource.resource_id
    resource_id = @new_resource.name
  else
    resource_id = @new_resource.resource_id
  end

  tags_to_delete = @new_resource.tags.keys

  tags_to_delete.each do |key|
    if @current_resource.tags.keys.include?(key) and @current_resource.tags[key] == @new_resource.tags[key]
      converge_by("delete tag '#{key}' on resource #{resource_id} with value '#{@current_resource.tags[key]}'") do
        ec2.delete_tags(resource_id, {key => @new_resource.tags[key]})
        Chef::Log.info("AWS: Deleted tag '#{key}' on resource #{resource_id} with value '#{@current_resource.tags[key]}'")
      end
    end
  end
end

action :force_remove do
  unless @new_resource.resource_id
    resource_id = @new_resource.name
  else
    resource_id = @new_resource.resource_id
  end

  @new_resource.tags.keys do |key|
    if @current_resource.tags.keys.include?(key)
      converge_by("AWS: Deleted tag '#{key}' on resource #{resource_id} with value '#{@current_resource.tags[key]}'") do
        ec2.delete_tags(resource_id, key)
        Chef::Log.info("AWS: Deleted tag '#{key}' on resource #{resource_id} with value '#{@current_resource.tags[key]}'")
      end
    end
  end
end

def load_current_resource
  @current_resource = Chef::Resource::AwsResourceTag.new(@new_resource.name)
  @current_resource.name(@new_resource.name)
  unless @new_resource.resource_id
    @current_resource.resource_id(@new_resource.name)
  else
    @current_resource.resource_id(@new_resource.resource_id)
  end

  @current_resource.tags(Hash.new)

  ec2.describe_tags(:filters => { 'resource-id' => @current_resource.resource_id }).map {
    |tag| @current_resource.tags[tag[:key]] = tag[:value]
  }

  @current_resource
end
