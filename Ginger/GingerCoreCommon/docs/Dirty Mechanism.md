# RepositoryItemBase dirty mechanism
When we view repository item in tree view and user can change field or properties in the EditPage we start tracking changes, as soon as change is detected we mark the item in the tree with the red modified icon

# When does tracking start?
When the item is selected in the tree and it's header is created

# How the tree view item header is updated?
The Tree node header is binded to the item name, DirtyStatusImage, SourceControlImage, when aproperty change event occur it will automatically apear

# What is being tracked?
All Field and Properties which are marked with [IsSerialziedForLocalRepository] - it is also must to create get/set with OnPropertyChanged event called, otherwise the change will not bubble up

# Are child itemas in Observable list tracked?
Yes, When RepositoryItem.StartTracking() is called it will drill down to Observable list

# When is the modified icon appear? turned off?
any change which trigger PropertyChanged event is checked against DirtyFieldsTracking if the properrty/field in the list it will be marked modified, when item is saved or undo it will be switched to 'NoChange', also if the item XML file is change in the file system, FileWatcher will detect the change and reload it, the item will be marked as 'Nochange'

# Where is the code
In RepositoryItemBase - DirtyStatus region
for tree view item - in TreeViewItemBase

# Be careful
If the repository item is having ObservableList<*> items and you do new ObservableList<*>(), the tracking will not be active on the list and changes in sub items will not trigger the is dirty of the parent, use clear, or trigger StartDirty manually

