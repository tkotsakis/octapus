   M S B A M L   `   `   `  ����T  PMahApps.Metro, Version=0.7.1.0, Culture=neutral, PublicKeyToken=f4fb5a3c4d1e5b4fV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  5   	   W SRelational.Octapus.WinClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:Relational.Octapus.WinClient.Converters'Relational.Octapus.WinClient.Converters 5   	   [2clr-namespace:Relational.Octapus.WinClient.Helpers$Relational.Octapus.WinClient.Helpers 5   	   u?clr-namespace:Relational.Octapus.WinClient.DependencyProperties1Relational.Octapus.WinClient.DependencyProperties 5
   	      WinClient.MainWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   c _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35Bi:http://schemas.microsoft.com/expression/2010/interactivity  5   	   Bei9http://schemas.microsoft.com/expression/2010/interactions  5   	   JControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro  5   	   @conv5clr-namespace:Relational.Octapus.WinClient.Converters  5   	   <hlp2clr-namespace:Relational.Octapus.WinClient.Helpers  5   	   Hdp?clr-namespace:Relational.Octapus.WinClient.DependencyProperties  5
   	   &   System.Windows.Media.TextOptions    TextFormattingMode$  Display=�5   	   (   "MahApps.Metro.Controls.MetroWindow   ShowIconOnTitleBar .  
������� � Title$ OCTAPUS��5   	    � WindowState$ Normal=�6    � WindowStartupLocation$ CenterScreen=�5      $	��532��5      $	��924��6$   $	��420��5      $	��600��6'   H  BRelational.Octapus.WinClient.DependencyProperties.WindowProperties   WindowClosing 5   	   �� *CommandExitApplication+ � 	Resources 5      K  ERelational.Octapus.WinClient.Converters.BooleanToRadiobuttonConverter !  booleanToRadiobuttonConverterG  ARelational.Octapus.WinClient.Converters.BoolToVisibilityConverter  boolToVisibilityConverterM  GRelational.Octapus.WinClient.Converters.BoolToHiddenVisibilityConverter # boolToHiddenVisibilityConverter<  6Relational.Octapus.WinClient.Converters.ValueConverter  valueConverter��%t   &	        &	      &	 &     &	 9       5   
   6-     5   
   6)     5   
   6/     5   
   6   5      .��5      � $��8q�6    � RowDefinitions 5   
   �� 5      $	��100 �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5       $��* �6   �� 5!      $��* �6   �� 5"      $
��Auto �6    � ColumnDefinitions 5%   
   �� 5&      $
��Auto �6   �� 5'      $	��50* �6   �� 5(      $
��Auto �6   �� 5)      $
��Auto �6   �� 5*      $	��50* �6   .+�50   
   �� $��Select workspace to build��6   $��1��64   $��0��6A   $��5q�6Q   �� 51   
   -   5        $��1��51      $��1��6    $��5q�6}   ��60   �� *
Workspaces+	 �� SelectedItem	 6S   �� *WorkspaceToBuild+6�   �� 53   
   $��1��6   $��2��6"   $��5q�62   {�.  .+�54      	� 
 	� 	GroupName$
 build��6   $!��Auto generated build number��6,    d� 	IsChecked$
 TrueY�6R   	� 55      $
 build��6   $��Manual entry��6,    6D   ��  �� Path$ AutoBuildNumber� �� Mode$ TwoWay=� �� 	Converter# [   �� ConverterParameter$ false��56      �� 58   
   $��Build Number��6   $��3��6'   $��5,67,5,3q�67   �� 59   
   $��Executable Version Number��6   $��3��64   $��	6,37,4,38q�6D   �� 5;   
   $��4��6   *   $MahApps.Metro.Controls.TextboxHelper   ClearTextButton . $��4,67,6,3q�5?      $��30��6   ��5<      �� *BuildNumber+{�5=      �� *AutoBuildNumber+# [  $ false��5?      �� 5A   
   -   5        $��4��5A      {�.  . $��	4,36,6,37q�5D      ��6    �� *ExecutableBuildNumber+ �� UpdateSourceTrigger$ PropertyChanged=�.��6�   ��5E      �� 5F       �� Triggers 5G      �� 5H       �� Binding 6&   �� $ ExecutableBuildNumber�# [  �� Value true6   .a�5I      ��  �� Property � �� �� Value
 �  ��6Q   5J      5L      .��5M      5N      � 5Q   
   $��3��5R      $��0��5S      $��5��5T      $��Stretch=�5U      $��5q�5V       5W      �� 5X      $	��50* �6#   �� 5Y      $
��Auto �6#   �� 5Z      $	��50* �6#   .+�5]      �� -   5        $ ��RemainingLibraries_ListBox��5_      $��0��5^       �� SelectionMode$ Multiple=�5a      ��5`      �� *RemainingLibraries+��5c      �� 5d       �� 
TargetType" ��5e      .�5f      ��  �@��
IsSelected 5h      �� *
IsSelected+$ TwoWay=�5i      ��5k      �� 5l      .R�5m      �� ��6$   �� *Value+5n      5p      �� 5r      $��1��6   .+�5t      �� $��>��6   ��6(   �� *CommandSelectLibraries+�� 5u      $��<��6   ��6(   �� *CommandRemoveLibraries+�� 5v      $��>>��6   ��6,   �� *CommandSelectAllLibraries+�� 5w      $��<<��6   ��6,   �� *CommandRemoveAllLibraries+5x      �� 5z      -   5        $��SelectedLibraries_ListBox��5{      $ Multiple=�5}      $��2��5~      ��5|      �� *LibrariesToBuild+��5      �� 5�      " ��5�      .�5�   &   ��  �@��
IsSelected 5�      �� *
IsSelected+$ TwoWay=�5�   #   ��5�      �� 5�      .R�5�      �� ��6$   �� *Value+5�      5�      5�      � 5�      $��4��5�      $��0��5�      $��5��5�       5�      �� 5�      $��* �6    �� 5�      $
��Auto �6    .+�5�      �� -   5         ��Name$ Logger_ListBox��5�      $��0��5�      $��5q�5�      ��5�      �� *
LoggerData+��5�      �� 5�      .R�5�      ��  �� Orientation$ 
Horizontal=�6%   .+�5�      �� $��2q�6E   ��6(   �� $ Logger�6E   �� 5�      $��2q�6C   ��6(   �� $
 Type�6C   �� 5�      $��2q�6F   ��6(   �� $ Message�6F   5�      5�      5�      �� 5�      $��1��5�      $��Center=�5�      y�5�      �� *IsBusy+# [ .+�5�      .	   (MahApps.Metro.Controls.ProgressIndicator	  $��4��6-     AccentColorBrush 	  ProgressColour# �  68   $	��320��6l   5�      �� 5�      $��1��6   $ 
Horizontal=�6&   $��Right=�6?   $��0,35,47,-35q�6[   .+�5�      �� $��Reset��6   $��5q�6O   ��6)   �� $ CommandReset�6O   5�      �� 5�      $��1��6   $ 
Horizontal=�6&   $��Right=�6?   $��0,35,102,-35q�6[   $��56��6q   .+�5�      �� $��Build��6   $��5q�6O   $��45��6Z   ��6)   �� $ CommandBuild�6Z   5�      5�      �� 5�   
   -   5        $��2��5�      $��4��5�      $��5q�5�       5�      �� *StopIfCheckedOut+.��5�      �� $
��Wrap=�6   $"��Search for check out objects��6,   5�      �� 5�   
   -   5        $��1��5�      $��4��5�      $��5q�5�       5�      �� *PrepareWorkableClient+.��5�      �� $��NoWrap=�6   $��Prepare workable client��6.   5�      �� 5�   
   $��5��6   $��3��6"   $��2��62   $ 
Horizontal=�6F   $��Right=�6_   .+�5�      �� $
��Exit��6   $��5q�6T   ��6$   �� $ CommandExitApplication�6T   5�      �� 5�   
   -   5        $��image1��5�   8   $	��100��6   $
��Left=�6   $
��Fill=�6F   $	��Top=�6U   $	��141��6m   $C��=/Relational.Octapus.WinClient;component/Resources/octapus.jpg��6y   5�      5�      