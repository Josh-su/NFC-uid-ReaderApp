   M S B A M L   `   `   `  ����D  @NFCReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:NFCReader	NFCReader  5   	       NFCReader.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   )localclr-namespace:NFCReader     5   	     � Title$  NFC UID Reader��5   	   $	��452��6    $	��400��6-   .��5	      � $��10q�6    � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   �� $��NFC UID Reader��6   $��16(�6)   $
��Bold"�67   $��Center=�6I   $��0��6f   �� 5   
   $��	Card UID:��6   $��	10,10,0,0q�6%   $��1��68   �� 5   
   -   5        $��txtUID��5       �� 
IsReadOnly$
 True��6    $��0,5,0,10q�62   $��25��6D   $��2��6P   �� 5!   
   $��
User Name:��6   $��10,5,0,0q�6&   $��3��68   � 5$   
   $��4��6    � ColumnDefinitions 5%      �� 5&      $��* �6#   �� 5'      $
��Auto �6#   .+�5)      �� -   5        $��txtUserName��5)      $��25��6)   ${�False��65   $��0��6G   $��Center=�6W   �� 5*      -   5         ��Name$ 
btnSaveUID��5*      $��Save UID��6)   $	��111��6<   $��30��6H   ${�False��6m   $��5,0,0,0q�6   $��1��6�   $��Center=�6�   5+      �� 5-   
   $��Logs:��6   $��5��6!   $��10,5,0,0q�6.   �� 5.   
   -   5        $��txtlogs��5.      $
 True��6!   $��0,5,0,10q�63   $	��100��6E    �� TextWrapping$
 Wrap=�6R   " �� VerticalScrollBarVisibility$
 Auto=�6f   $��6��6�   �� 5/   
   -   5        $��lblReaderStatus��5/      $��Reader Status:��6'   $��7��6@   $��10,5,0,0q�6M   $��Center=�6_   �� 52   
    �� Orientation$ 
Horizontal=�6   $��8��6.   $��Center=�6;   $��10q�6X   .+�53      �� -   5        $��btnCreateNewTsv��53      $��Create New TSV File��6,   $	��150��6J   $��30��6V   6b   �� 54      -   5        $��btnOpenTsvDirectory��54      $��Open TSV Directory��60   $	��150��6M   $��30��6Y   $��5,0,0,0q�6�   55      �� 58   
   -   5        $��lblCurrentFile��58      $��Current TSV File:��6&   $��0,10,0,0q�6B   $��Center=�6T   $��9��6q   59      5:      