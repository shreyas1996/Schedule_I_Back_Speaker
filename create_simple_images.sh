#!/bin/bash

# Create sophisticated album art with gradients and musical symbols
echo "Creating enhanced album art images with gradients and musical notes..."

# Function to create an image with gradient background and musical symbols
create_album_art() {
    local filename=$1
    local r1=$2 g1=$3 b1=$4    # Start color
    local r2=$5 g2=$6 b2=$7    # End color
    local symbol_type=$8        # Type of musical symbol to add
    
    echo "Creating $filename with gradient and $symbol_type symbols..."
    
    # Create PPM header
    printf "P3\n120 120\n255\n" > temp.ppm
    
    # Generate gradient background with mathematical progression
    for ((y=0; y<120; y++)); do
        for ((x=0; x<120; x++)); do
            # Calculate gradient factor (0.0 to 1.0)
            # Use radial gradient from center
            dx=$((x - 60))
            dy=$((y - 60))
            dist=$((dx*dx + dy*dy))
            
            # Normalize distance (max distance from center is about 85)
            if [ $dist -gt 7200 ]; then
                factor=100
            else
                factor=$((dist * 100 / 7200))
            fi
            
            # Interpolate colors
            r=$(( r1 + (r2 - r1) * factor / 100 ))
            g=$(( g1 + (g2 - g1) * factor / 100 ))
            b=$(( b1 + (b2 - b1) * factor / 100 ))
            
            # Add musical symbols based on position
            symbol_added=0
            
            case $symbol_type in
                "note")
                    # Add musical note at center-left
                    if [ $x -ge 40 ] && [ $x -le 50 ] && [ $y -ge 55 ] && [ $y -le 65 ]; then
                        r=255; g=255; b=255; symbol_added=1  # White note head
                    elif [ $x -ge 48 ] && [ $x -le 50 ] && [ $y -ge 40 ] && [ $y -le 55 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Note stem
                    elif [ $x -ge 70 ] && [ $x -le 80 ] && [ $y -ge 50 ] && [ $y -le 60 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Second note
                    elif [ $x -ge 78 ] && [ $x -le 80 ] && [ $y -ge 35 ] && [ $y -le 50 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Second note stem
                    fi
                    ;;
                "vinyl")
                    # Add vinyl record rings
                    if [ $dist -ge 2800 ] && [ $dist -le 3200 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Outer ring
                    elif [ $dist -ge 1600 ] && [ $dist -le 2000 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Middle ring
                    elif [ $dist -ge 400 ] && [ $dist -le 600 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Inner ring
                    elif [ $dist -le 100 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Center hole
                    fi
                    ;;
                "headphones")
                    # Add proper headphone shape with connectors
                    # Headband arc (top curve)
                    for arc_x in $(seq 30 90); do
                        arc_y=$((35 + (arc_x-60)*(arc_x-60)/80))  # Parabolic curve
                        if [ $x -ge $((arc_x-2)) ] && [ $x -le $((arc_x+2)) ] && [ $y -ge $((arc_y-2)) ] && [ $y -le $((arc_y+2)) ]; then
                            r=255; g=255; b=255; symbol_added=1
                        fi
                    done
                    
                    # Left connector (from headband to left ear cup)
                    if [ $x -ge 28 ] && [ $x -le 32 ] && [ $y -ge 55 ] && [ $y -le 75 ]; then
                        r=255; g=255; b=255; symbol_added=1
                    fi
                    
                    # Right connector (from headband to right ear cup)
                    if [ $x -ge 88 ] && [ $x -le 92 ] && [ $y -ge 55 ] && [ $y -le 75 ]; then
                        r=255; g=255; b=255; symbol_added=1
                    fi
                    
                    # Left ear cup (circular)
                    left_dx=$((x - 30))
                    left_dy=$((y - 80))
                    if [ $((left_dx*left_dx + left_dy*left_dy)) -le 120 ] && [ $((left_dx*left_dx + left_dy*left_dy)) -ge 80 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Hollow circle
                    fi
                    
                    # Right ear cup (circular)
                    right_dx=$((x - 90))
                    right_dy=$((y - 80))
                    if [ $((right_dx*right_dx + right_dy*right_dy)) -le 120 ] && [ $((right_dx*right_dx + right_dy*right_dy)) -ge 80 ]; then
                        r=255; g=255; b=255; symbol_added=1  # Hollow circle
                    fi
                    ;;
                "waves")
                    # Add proper curved sound wave patterns
                    # Calculate sine-like wave patterns emanating from left side
                    wave_source_x=20
                    wave_source_y=60
                    
                    # Create multiple concentric wave arcs
                    for wave_radius in 20 35 50 65; do
                        dx=$((x - wave_source_x))
                        dy=$((y - wave_source_y))
                        dist_sq=$((dx*dx + dy*dy))
                        target_sq=$((wave_radius * wave_radius))
                        
                        # Create arc segments (right side only)
                        if [ $x -gt $wave_source_x ] && [ $dist_sq -ge $((target_sq - 60)) ] && [ $dist_sq -le $((target_sq + 60)) ]; then
                            # Only show right half of the circles (sound waves going right)
                            if [ $dx -gt 0 ]; then
                                r=255; g=255; b=255; symbol_added=1
                            fi
                        fi
                    done
                    
                    # Add some horizontal wave lines for variety
                    for wave_y in 45 60 75; do
                        # Create wavy horizontal lines
                        wave_offset=$((5 * (x-30) * (x-30) / 400))  # Parabolic curve
                        actual_y=$((wave_y + wave_offset))
                        if [ $x -ge 35 ] && [ $x -le 100 ] && [ $y -ge $((actual_y-1)) ] && [ $y -le $((actual_y+1)) ]; then
                            r=255; g=255; b=255; symbol_added=1
                        fi
                    done
                    ;;
                "play")
                    # Add YouTube-style play button triangle
                    # Triangle points: (45,35), (45,85), (85,60)
                    if [ $x -ge 45 ] && [ $y -ge 35 ] && [ $y -le 85 ]; then
                        # Check if point is inside triangle
                        # Using simple line equations
                        top_line=$((35 + (x-45)*25/40))    # Top edge
                        bottom_line=$((85 - (x-45)*25/40)) # Bottom edge
                        if [ $y -ge $top_line ] && [ $y -le $bottom_line ] && [ $x -le 85 ]; then
                            r=255; g=255; b=255; symbol_added=1
                        fi
                    fi
                    ;;
                "folder")
                    # Add folder shape with musical note
                    # Folder body
                    if [ $x -ge 25 ] && [ $x -le 95 ] && [ $y -ge 45 ] && [ $y -le 85 ]; then
                        r=255; g=255; b=255; symbol_added=1
                    fi
                    # Folder tab
                    if [ $x -ge 25 ] && [ $x -le 55 ] && [ $y -ge 35 ] && [ $y -le 45 ]; then
                        r=255; g=255; b=255; symbol_added=1
                    fi
                    # Musical note inside folder
                    if [ $x -ge 55 ] && [ $x -le 65 ] && [ $y -ge 60 ] && [ $y -le 70 ]; then
                        r=$r1; g=$g1; b=$b1; symbol_added=1  # Note head in original color
                    elif [ $x -ge 63 ] && [ $x -le 65 ] && [ $y -ge 50 ] && [ $y -le 60 ]; then
                        r=$r1; g=$g1; b=$b1; symbol_added=1  # Note stem
                    fi
                    ;;
            esac
            
            # Ensure RGB values are within bounds
            [ $r -lt 0 ] && r=0; [ $r -gt 255 ] && r=255
            [ $g -lt 0 ] && g=0; [ $g -gt 255 ] && g=255
            [ $b -lt 0 ] && b=0; [ $b -gt 255 ] && b=255
            
            echo "$r $g $b"
        done
    done >> temp.ppm
    
    # Convert to PNG using sips
    sips -s format png temp.ppm --out "EmbeddedResources/AlbumArt/$filename" 2>/dev/null
    echo "✓ Created $filename with gradient background and $symbol_type symbols"
}

# Create theme-specific album art with gradients and symbols
echo "🎨 Creating theme-specific album art..."

# Local folder - Green gradient with folder symbol
create_album_art "local_folder.png" 20 80 40 60 140 80 "folder"

# Jukebox - Purple/pink gradient with musical notes  
create_album_art "jukebox_retro.png" 80 20 60 140 60 120 "note"

# YouTube - Red gradient with play button
create_album_art "youtube_play.png" 180 0 0 255 60 60 "play"

echo "🎵 Creating generic album art variations..."

# Generic variations with different symbols
create_album_art "generic_vinyl.png" 60 20 80 120 60 140 "vinyl"
create_album_art "generic_headphones.png" 80 60 20 140 120 60 "headphones"
create_album_art "generic_waves.png" 20 60 80 60 120 140 "waves"

# Clean up
rm temp.ppm

echo ""
echo "🎉 All enhanced album art images created successfully!"
echo "📁 Album art features:"
echo "   • Radial gradient backgrounds"
echo "   • Musical symbols and icons"
echo "   • Theme-specific designs"
echo "   • High contrast white symbols"
echo ""
ls -la EmbeddedResources/AlbumArt/ 