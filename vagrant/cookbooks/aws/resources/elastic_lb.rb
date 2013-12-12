actions :register, :deregister

attribute :aws_access_key,        :kind_of => String
attribute :aws_secret_access_key, :kind_of => String
attribute :name,                  :kind_of => String

def initialize(*args)
  super
  @action = :register
end
