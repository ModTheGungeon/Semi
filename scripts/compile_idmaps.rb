#!/usr/bin/env ruby

VALID_TYPES = [:items, :enemies, :synergies]

INDENT_STR = "\t"
$indent = 0
$outfile = File.open(File.join(__dir__, "..", "Semi", "src", "Generated", "GeneratedIDMaps.cs"), "w")

def sline(txt = nil)
	if !txt 
		line(nil)
		return
	end
	line(txt + ";")
end

def line(txt)
	$outfile.puts (INDENT_STR * $indent) + (txt || "")
end

def indent
	$indent += 1
end

def dedent
	$indent -= 1
	$indent = 0 if $indent < 0
end

def with_indent
	indent
	yield 
	dedent
end

def id_pool_name
	case $type
	when :items; return "Items"
	when :enemies; return "Enemies"
	when :synergies; return "Synergies"
	end
end

def retrieve_entry_call(index)
	case $type
	when :items; return "PickupObjectDatabase.GetById(#{index})"
	when :enemies; return "EnemyDatabase.Instance.Entries[#{index}].GetPrefab<AIActor>()"
	when :synergies; return "GameManager.Instance.SynergyManager.synergies[#{index}]"
	end
end

def extra_predef_vars
	case $type
	when :synergies; return "Semi.Patches.AdvancedSynergyEntry syn"
	end
end

def post_call_statements(id, item_var_name)
	case $type
	when :items; return "((Semi.Patches.PickupObject)#{item_var_name}).UniqueItemID = id;"
	when :enemies; return nil
	when :synergies; return <<-EOF
			syn = (Semi.Patches.AdvancedSynergyEntry)#{item_var_name};

			syn.OptionalGuns = SemiLoader.ConvertItemIDList(syn.OptionalGunIDs);
			syn.MandatoryGuns = SemiLoader.ConvertItemIDList(syn.MandatoryGunIDs);
			syn.OptionalItems = SemiLoader.ConvertItemIDList(syn.OptionalItemIDs);
			syn.MandatoryItems = SemiLoader.ConvertItemIDList(syn.MandatoryItemIDs);
			syn.UniqueID = id;

			syn.OptionalGunIDs = syn.MandatoryGunIDs = syn.OptionalItemIDs = syn.MandatoryItemIDs = null;
		EOF
	end
end

def entry_type
	case $type
	when :items; return "PickupObject"
	when :enemies; return "AIActor"
	when :synergies; return "AdvancedSynergyEntry"
	end
end

def gen_apply_method()
	line "public static void Apply#{id_pool_name}() {"
	with_indent do
		sline "ID id"
		sline "#{entry_type} item"
		extra_predefs = extra_predef_vars
		if extra_predefs
			extra_predefs.each_line do |l|
				next if l.strip.size == 0
				sline l.strip
			end
		end

		File.readlines($input).each do |l|
			next if l.start_with?("#") || l.strip.size == 0

			split_line = l.split(" ")
			tag = split_line[0]
			id = split_line[1].to_i
			name = split_line[2]

			sline "id = (ID)\"#{name}\""
			sline "item = #{retrieve_entry_call(id)}"
			sline "Registry.#{id_pool_name}.Add(id, item)"

			post = post_call_statements(id, "item")
			if post
				post.each_line do |l|
					next if l.strip.size == 0
					line l.strip
				end
			end

			sline
		end
	end
	line "}"
end

def set_file(path, type)
	$input = path
	$type = type
end

sline "using System"
sline "using Semi"
sline
line "namespace Semi.Generated {"
with_indent do
	line "public static class GeneratedIDMaps {"
	with_indent do
		apply_funcs = []
		filenames = []

		VALID_TYPES.each do |t|
			filename = "#{t}.txt"
			filenames << filename
			path = File.join(__dir__, "..", "Semi", "res", "idmaps", filename)
			set_file(path, t)
			gen_apply_method
			apply_funcs << "Apply#{id_pool_name}"
		end

		line "public static void Apply() {"
		with_indent do
			apply_funcs.each.with_index do |f, i|
				sline "SemiLoader.Logger.Debug(\"IDMap: #{filenames[i]} (precompiled)\")"
				sline "#{f}()"
			end
		end
		line "}"
	end
	line "}"
end
line "}"