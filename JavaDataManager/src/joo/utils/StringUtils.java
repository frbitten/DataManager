package joo.utils;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

public abstract class StringUtils {
	
	public enum StringOrientation
    {
        LeftRight,
        RightLeft
    }

    public static byte[] StringHexToByteArray(String hexValues)
    {
        if (hexValues.length() % 2 != 0)
        {
            throw new IllegalArgumentException("Tamanho de string invalido. Tem que possuir numero pares de caracteres. Pois um hexadecimal é composto de 2 caracteres variando de 0 a F.");
        }
        int index = 0;
        int count = 0;
        byte[] ret = new byte[hexValues.length() / 2];
        while (index < hexValues.length())
        {
            ret[count] = (byte) ((Character.digit(hexValues.substring(index, 2).charAt(0), 16) << 4) + Character.digit(hexValues.substring(index, 2).charAt(1), 16));
            index += 2;
            count++;
        }
        return ret;
    }

    public static String paddingSpaces(String main, String add, StringOrientation so){
    	return paddingSpaces(main,add,so,false);
    }
    
    public static String paddingSpaces(String main, String add, StringOrientation so, boolean abbreviate)
    {
        String spaceFree = " ";
        Integer[] sizes = getGroupSizesOfSpaces(main);
        boolean find = false;
        for (int size : sizes)
        {
            if (size > add.length())
            {
                find = true;
                break;
            }
            else
            {
                if (size > 4 && abbreviate)
                {
                    find = true;
                    break;
                }
            }

        }
        if (!find)
        {
            return main;
        }

        if (so == StringOrientation.LeftRight)
        {
            int size = 0;
            for (int i = 0; i < sizes.length; i++)
            {
                if (sizes[i] > add.length())
                {
                    size = sizes[i];
                    break;
                }
                else
                {
                    if (sizes[i] > 4 && abbreviate)
                    {
                        size = sizes[i];
                        break;
                    }
                }

            }

            spaceFree = repeat(' ', size);

            int index = main.indexOf(spaceFree);
            if (index < 0)
            {
                return main;
            }

            if (size < add.length())
            {
                int difference = add.length() - size;

                if (main.length() > index + (add.length() - difference) + 2)
                    add = add.substring(0, add.length() - (difference + 2) - 1) + "..";
                else
                    add = add.substring(0, add.length() - (difference + 2)) + "..";
            }

            return overrideString(main, add, index);
        }
        else
        {
            int size = 0;
            for (int i = sizes.length - 1; i >= 0; i--)
            {
                if (sizes[i] > add.length())
                {
                    size = sizes[i];
                    break;
                }
                else
                {
                    if (sizes[i] > 4 && abbreviate)
                    {
                        size = sizes[i];
                        break;
                    }
                }
            }

            spaceFree = repeat(' ', size);

            int index = main.lastIndexOf(spaceFree);
            if (index < 0)
            {
                return main;
            }

            if (size < add.length())
            {
                int difference = add.length() - size;
                add = add.substring(0, add.length() - (difference + 2)) + "..";
            }

            index += (spaceFree.length() - add.length());
            return overrideString(main, add, index);
        }
    }
    
    public static String repeat(char character, int size){
    	char[] chars = new char[size];
    	Arrays.fill(chars, character);
    	return new String(chars);
    }
    
    public static String insert(String main, String insert,int position){
    	String aux="";
        if(position>0){
        	aux=main.substring(0,position);
        }
    	aux+=insert;
    	aux+=main.substring(position,main.length());
    	return aux;
    }
    
    public static String overrideString(String main, String add, int start)
    {
    	String aux="";
        if(start>0){
        	aux=main.substring(0,start);
        }
    	aux+=add;
    	if(main.length()>start+add.length()){
    		aux+=main.substring(start+add.length(),main.length());
    	}
    	return aux;
    }
    private static Integer[] getGroupSizesOfSpaces(String main)
    {
        List<Integer> sizes = new ArrayList<Integer>();
        int nspaces = 0;
        for (int i = 0; i < main.length(); i++)
        {
            if (main.charAt(i) == ' ')
            {
                nspaces++;
            }
            else
            {
                sizes.add(nspaces);
                nspaces = 0;
            }
        }
        if (nspaces > 1)
        {
            sizes.add(nspaces);
        }
        return sizes.toArray(new Integer[sizes.size()]);
    }

    public static String center(String text, int rowSize, char characterToFill)
    {
        if (rowSize <= 0)
            throw new IllegalArgumentException("Row Size Invalid");

        if (text==null || text.equals(""))
        {
            return repeat(characterToFill,rowSize);
        }

        if (text.length() >= rowSize)
            return text;


        String bfspace = "";
        String afspace = "";
        int spacenumber;
        if ((rowSize - text.length()) % 2 == 0)
        {
            spacenumber = (rowSize - text.length()) / 2;
            for (int i = 0; i < spacenumber; i++)
            {
                bfspace = bfspace + characterToFill;
            }
            return bfspace+text+bfspace;
        }
        else
        {
            float numberLeftAndRight = (float)(rowSize - text.length()) / 2;
            spacenumber = Math.round(numberLeftAndRight);

            for (int i = 0; i < spacenumber; i++)
            {
                afspace = afspace + characterToFill;
            }
            for (int i = 0; i < spacenumber - 1; i++)
            {
                bfspace = bfspace + characterToFill;
            }
            return bfspace+text+afspace;
        }
    }
    public static String[] WrapText(String text, int maxCol)
    {
        List<String> ret = new ArrayList<String>();
        String[] aux = text.split("\n");
        for (String item : aux)
        {
            if (item.length() > maxCol)
            {
                String[] aux2 = item.split(" ");
                String line="";
                for (String item2 : aux2)
                {
                    if (line.length() + item2.length() + 1 > maxCol)
                    {
                        ret.add(line);
                        line = "";
                    }
                    else
                    {
                        if (line != "")
                        {
                            line += " ";
                        }
                    }
                    line += item2;
                }
                ret.add(line);
                line = "";
            }
            else
            {
                ret.add(item);
            }
        }
        return ret.toArray(new String[ret.size()]);
    }
}
