###
# script for converting the Phonocolor data to the format used in TELL in Motion
# e.g. : mot,m|O,m|ot
###

import pandas as pd

INPUT_FILE = "word_to_phoneme.csv"
OUTPUT_FILE = "dict.csv"

# read the csv export of the Phonocolor sql dump
# the table to export is `word_to_phoneme`
# must be exported with the following parameters:
#    only export 3 columns: word, phonemes, colors
#    columns separated with ;
#    columns enclosed with "
#    columns escaped with "
#    lines terminated with AUTO
#    replace NULL with NULL
#    encoding is utf-8
df = pd.read_csv(INPUT_FILE, sep=";", header=None)
df.columns = ["word", "phonemes", "colors"]
df.dropna(subset=['colors', 'phonemes'], inplace=True)
df.colors = df.colors.str.replace('"', '')
df.colors = df.colors.str.replace('{', '[')
df.colors = df.colors.str.replace('}', ']')

# define the cleaning function
def clean(row):
    word = row.word
    phonemes = row.phonemes.split(" ")
    colors = row.colors
    letters = colors[2:-2].split("],[")
    grapheme_out = ""
    last = ""
    phoneme_list = []
    for c,letter in zip(list(word),letters):
        if c.isalpha(): # skip apostrophe
            was_dict = ":" in letter # check if was an object cause sometimes keys are swapped
            parts = letter.split(",")
            if was_dict:
                parts = sorted(parts)
            parts = [p.split(":")[-1] for p in parts] # remove parts before ":"
            if len(parts) == 4: # [c, col, col, col] or [0:c, 1:col, 2:col, 3:col]
                parts = parts[1:3]
            elif len(parts) == 3: # [c, col, col] or [0:c, 1:col, 3:col]
                parts = parts[1:]
            elif len(parts) == 2: # [c, col] or [0:c, 1:col]
                parts = parts[1:] # otherwise keep parts as they are, [0:col] or [c] or whatever
            parts = [p for p in parts if "#" in p] # filter out only colors
            color = ",".join(parts) # order matters for double colors
            nb_colors = len(set(parts))    
            if parts and color != last: # if no color, assume previous one
                if grapheme_out:
                    grapheme_out += "|"
                if nb_colors == 2 and phonemes[0] in "NJ":
                    nb_colors = 1
                head, phonemes = phonemes[:nb_colors], phonemes[nb_colors:]
                phoneme_list.append(" ".join(head))
            elif parts and color == last: # double phonemes
                if phoneme_list and phonemes and phoneme_list[-1] == phonemes[0]:
                    head, phonemes = phonemes[:1], phonemes[1:]
                    phoneme_list.append(" ".join(head))
                    grapheme_out += "|"
            last = color
        grapheme_out += c
        
    if phonemes:
        if len(phonemes) == 1 and phonemes[0] == "e": # handle at least some trailing e phonems
            if phoneme_list[-1] == "j":
                phoneme_list[-1] = "j e"
                phonemes = []
        
    phoneme_out = "|".join(phoneme_list)
    
    # TODO: check if still some phonemes left
    return [phoneme_out, grapheme_out, "|".join(phonemes)]

# apply the cleaning function
df[['clean_phonemes', 'graphemes', 'excess']] = df.apply(clean, axis=1, result_type='expand')

# store the new values in file
df[['word','clean_phonemes','graphemes']].to_csv(OUTPUT_FILE, index=False)